using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public static class FileHandler
{
    public static List<GeneticData> LoadGeneticData(string filename)
    {
        List<GeneticData> geneticDataList = new List<GeneticData>();

        if (!File.Exists(filename))
        {
            throw new FileNotFoundException($"File {filename} not found.");
        }

        string[] lines = File.ReadAllLines(filename);
        foreach (string line in lines)
        {
            string[] parts = line.Split('\t');
            if (parts.Length == 3)
            {
                GeneticData data = new GeneticData
                {
                    protein = parts[0].Trim(),
                    organism = parts[1].Trim(),
                    amino_acids = RLDecoding(parts[2].Trim())
                };

                // Validate amino acid sequence
                if (!IsValidAminoAcidSequence(data.amino_acids))
                {
                    Console.WriteLine($"Warning: Invalid amino acid sequence in {data.protein}. Skipping.");
                    continue;
                }

                geneticDataList.Add(data);
            }
        }

        return geneticDataList;
    }

    public static string[] LoadCommands(string filename)
    {

    }

    public static void WriteResults(string filename, string content)
    {
       
    }

    private static bool IsValidAminoAcidSequence(string sequence)
    {
        const string validAminoAcids = "ACDEFGHIKLMNPQRSTVWY";
        foreach (char c in sequence)
        {
            if (validAminoAcids.IndexOf(char.ToUpper(c)) == -1)
                return false;
        }
        return true;
    }

    private static string RLDecoding(string encoded)
    {
        StringBuilder decoded = new StringBuilder();
        int i = 0;

        while (i < encoded.Length)
        {
            if (char.IsDigit(encoded[i]))
            {
                int count = int.Parse(encoded[i].ToString());
                if (i + 1 < encoded.Length)
                {
                    char aminoAcid = encoded[i + 1];
                    decoded.Append(aminoAcid, count);
                    i += 2;
                }
                else
                {
                    decoded.Append(encoded[i]);
                    i++;
                }
            }
            else
            {
                decoded.Append(encoded[i]);
                i++;
            }
        }

        return decoded.ToString();
    }
}

