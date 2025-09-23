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
        for (int lineNumber = 0; lineNumber < lines.Length; lineNumber++) // Line number tracking
        {
            string line = lines[lineNumber];

            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] parts = line.Split('\t');
            if (parts.Length != 3) // Validation for exactly 3 parts
            {
                Console.WriteLine($"Warning: Line {lineNumber + 1} has incorrect format. Expected 3 tab-separated parts, got {parts.Length}. Skipping."); // NEW: error message with line number
                continue;
            }

            try
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
                    Console.WriteLine($"Warning: Line {lineNumber + 1} has invalid amino acid" +
                        $" sequence in {data.protein}. Skipping."); // Error message with line number
                    continue;
                }

                geneticDataList.Add(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Error processing line {lineNumber + 1}:" +
                    $" {ex.Message}. Skipping."); // Error handling with line number
                continue;
            }
        }

        return geneticDataList;
    }

    public static string[] LoadCommands(string filename)
    {
        if (!File.Exists(filename))
        {
            throw new FileNotFoundException($"File {filename} not found.");
        }

        return File.ReadAllLines(filename);
    }

    public static void WriteResults(string filename, string content)
    {
        File.WriteAllText(filename, content);
        Console.WriteLine($"Results written to {filename}");
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

    private static string RLDecoding(string encoded) //
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

