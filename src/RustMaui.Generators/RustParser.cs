using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace RustMaui.Generators;

internal record RustExport(
    string Name,
    string CSharpName,
    string ReturnType,
    string CSharpParams,
    bool HasUnknownTypes);

internal static class RustParser
{
    // Matches: #[no_mangle]\n[optional attrs\n]pub extern "C" fn name(params) [-> ret] {
    private static readonly Regex FnRegex = new(
        @"#\[no_mangle\](?:\s*#\[[^\]]*\])*\s*pub\s+extern\s+""C""\s+fn\s+(\w+)\s*\(([^)]*)\)\s*(?:->\s*([^{]+?))?\s*\{",
        RegexOptions.Compiled | RegexOptions.Singleline);

    private static readonly Dictionary<string, string> TypeMap = new()
    {
        // Rust primitives
        ["i8"]   = "sbyte",  ["u8"]   = "byte",
        ["i16"]  = "short",  ["u16"]  = "ushort",
        ["i32"]  = "int",    ["u32"]  = "uint",
        ["i64"]  = "long",   ["u64"]  = "ulong",
        ["f32"]  = "float",  ["f64"]  = "double",
        ["isize"] = "nint",  ["usize"] = "nuint",
        // std::os::raw / libc aliases
        ["c_char"]   = "sbyte",
        ["c_schar"]  = "sbyte",  ["c_uchar"]  = "byte",
        ["c_short"]  = "short",  ["c_ushort"]  = "ushort",
        ["c_int"]    = "int",    ["c_uint"]    = "uint",
        ["c_long"]   = "int",    ["c_ulong"]   = "uint",
        ["c_float"]  = "float",  ["c_double"]  = "double",
        // Opaque pointers
        ["*mut c_void"]   = "IntPtr",
        ["*const c_void"] = "IntPtr",
        ["*mut u8"]       = "IntPtr",
        ["*const u8"]     = "IntPtr",
        ["*mut i8"]       = "IntPtr",
        ["*const i8"]     = "IntPtr",
    };

    public static List<RustExport> ParseExports(string source)
    {
        var exports = new List<RustExport>();
        foreach (Match m in FnRegex.Matches(source))
        {
            var name      = m.Groups[1].Value;
            var rawParams = m.Groups[2].Value.Trim();
            var rawRet    = m.Groups[3].Success ? m.Groups[3].Value.Trim() : string.Empty;

            bool hasUnknown = false;
            var returnType  = MapReturnType(rawRet, ref hasUnknown);
            var csParams    = MapParams(rawParams, ref hasUnknown);

            exports.Add(new RustExport(
                Name:            name,
                CSharpName:      ToPascalCase(name),
                ReturnType:      returnType,
                CSharpParams:    csParams,
                HasUnknownTypes: hasUnknown));
        }
        return exports;
    }

    private static string MapReturnType(string rust, ref bool hasUnknown)
    {
        if (string.IsNullOrWhiteSpace(rust)) return "void";
        var t = rust.Trim();
        if (TypeMap.TryGetValue(t, out var mapped)) return mapped;
        hasUnknown = true;
        return t;
    }

    private static string MapParams(string raw, ref bool hasUnknown)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;

        var parts = new List<string>();
        int idx = 0;
        foreach (var segment in raw.Split(','))
        {
            var trimmed = segment.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;

            string paramName, rustType;
            var colon = trimmed.IndexOf(':');
            if (colon >= 0)
            {
                paramName = trimmed.Substring(0, colon).Trim();
                rustType  = trimmed.Substring(colon + 1).Trim();
            }
            else
            {
                paramName = $"arg{idx}";
                rustType  = trimmed;
            }

            // Strip leading underscores (_canvas → canvas), lone _ → argN
            paramName = paramName.TrimStart('_');
            if (string.IsNullOrEmpty(paramName)) paramName = $"arg{idx}";

            paramName = EscapeKeyword(ToCamelCase(paramName));
            var csType = MapSingleType(rustType, ref hasUnknown);
            parts.Add($"{csType} {paramName}");
            idx++;
        }
        return string.Join(", ", parts);
    }

    private static string MapSingleType(string rust, ref bool hasUnknown)
    {
        var t = rust.Trim();
        if (TypeMap.TryGetValue(t, out var mapped)) return mapped;
        hasUnknown = true;
        return t;
    }

    private static string ToPascalCase(string snake)
    {
        var parts = snake.TrimStart('_').Split('_');
        var sb = new StringBuilder();
        foreach (var p in parts)
        {
            if (p.Length == 0) continue;
            sb.Append(char.ToUpperInvariant(p[0]));
            if (p.Length > 1) sb.Append(p.Substring(1));
        }
        return sb.Length > 0 ? sb.ToString() : snake;
    }

    private static string ToCamelCase(string snake)
    {
        var parts = snake.Split('_');
        if (parts.Length == 0) return snake;
        var sb = new StringBuilder(parts[0]);
        for (int i = 1; i < parts.Length; i++)
        {
            if (parts[i].Length == 0) continue;
            sb.Append(char.ToUpperInvariant(parts[i][0]));
            if (parts[i].Length > 1) sb.Append(parts[i].Substring(1));
        }
        return sb.ToString();
    }

    private static string EscapeKeyword(string name) => name switch
    {
        "out" or "ref" or "in" or "object" or "string" or
        "class" or "event" or "base" or "this" or "params" => "@" + name,
        _ => name,
    };
}
