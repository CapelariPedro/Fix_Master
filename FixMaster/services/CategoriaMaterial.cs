using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace FixMaster.Services;

public static class CategoriaMaterial
{
    public static IReadOnlyList<string> Tipos { get; } =
    [
        "Peças",
        "Ferramentas",
        "Insumos",
        "Equipamentos",
        "Outros"
    ];

    public static bool TentarObterPrefixo(
        string? tipo,
        string? tipoInsumo,
        out string prefixo)
    {
        prefixo = tipo switch
        {
            "Peças" => "PEC",
            "Ferramentas" => "FER",
            "Insumos" => tipoInsumo switch
            {
                "Elétrico" => "INS-E",
                "Hidráulico" => "INS-H",
                _ => ""
            },
            "Equipamentos" => "EQP",
            "Outros" => "OUT",
            _ => ""
        };
        return prefixo.Length > 0;
    }

    public static string GerarCodigo(string prefixo) =>
        $"{prefixo}-{RandomNumberGenerator.GetInt32(100000, 1000000)}";
}
