using System;
using System.Collections.Generic;
using System.Text;

// Structure to store genetic data
public struct GeneticData
{
    public string protein;
    public string organism;
    public string amino_acids;
}

class Program
{
    // List to store all genetic data
    static List<GeneticData> geneticDataList = new List<GeneticData>();

    // Valid amino acid characters (20 standard amino acids)
    static readonly string validAminoAcids = "ACDEFGHIKLMNPQRSTVWY";

    static void Main(string[] args)
    {
        Console.WriteLine("Genetic Data Analysis Program");
        Console.WriteLine("==============================");

        // Load genetic data
        try
        {
            // Load genetic data from file
            geneticDataList = FileHandler.LoadGeneticData("C:\\Users\\Lenovo\\source\\repos\\Project1\\file\\sequences.txt");
            Console.WriteLine($"Loaded {geneticDataList.Count} valid protein records.");

            // Load commands from file
            string[] commands = FileHandler.LoadCommands("C:\\Users\\Lenovo\\source\\repos\\Project1\\file\\commands.txt");
            Console.WriteLine($"Loaded {commands.Length} commands.");

            // Process commands and generate output
            string output = ProcessCommands(commands);

            // Write results to file
            FileHandler.WriteResults("C:\\Users\\Lenovo\\source\\repos\\Project1\\file\\results.txt", output);

            Console.WriteLine("Processing completed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    static string ProcessCommands(string[] commands)
    {
        StringBuilder output = new StringBuilder();
        output.AppendLine("Genetic Data Analysis Results");
        output.AppendLine("==============================");
        output.AppendLine();

        int commandNumber = 1;
        foreach (string commandLine in commands)
        {
            if (string.IsNullOrWhiteSpace(commandLine))
                continue;

            string[] parts = commandLine.Split('\t');
            if (parts.Length == 0)
                continue;

            string operation = parts[0].Trim().ToLower();
            string result = $"{commandNumber:D3}: {commandLine}\n";

            try
            {
                switch (operation)
                {
                    case "search":
                        if (parts.Length >= 2)
                        {
                            string searchSequence = RLDecoding(parts[1].Trim());
                            result += SearchSequence(searchSequence);
                        }
                        break;
                    case "diff":
                        if (parts.Length >= 3)
                        {
                            result += DiffProteins(parts[1].Trim(), parts[2].Trim());
                        }
                        break;
                    case "mode":
                        if (parts.Length >= 2)
                        {
                            result += ModeProtein(parts[1].Trim());
                        }
                        break;
                    default:
                        result += "Unknown command";
                        break;
                }
            }
            catch (Exception ex)
            {
                result += $"Error processing command: {ex.Message}";
            }

            output.AppendLine(result);
            output.AppendLine(new string('-', 50));
            output.AppendLine();

            commandNumber++;
        }
        return output.ToString();
    }

    static void DecodeSequence()
    {
        Console.Write("Enter the RLE-encoded sequence to decode: ");
        string input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("Invalid input. Sequence cannot be empty.");
            return;
        }

        string decoded = RLDecoding(input);
        Console.WriteLine($"Decoded sequence: {decoded}");
    }

    //Search for amino acid sequence in loaded proteins
    static string SearchSequence(string searchSequence)
    {
        StringBuilder result = new StringBuilder();
        bool found = false;

        foreach (GeneticData data in geneticDataList)
        {
            if (data.amino_acids.Contains(searchSequence))
            {
                result.AppendLine($"{data.organism} {data.protein}");
                found = true;
            }
        }

        if (!found)
        {
            result.AppendLine("NOT FOUND");
        }

        return result.ToString();
    }

    // DIFF function: Compare two proteins
    static string DiffProteins(string protein1, string protein2)
    {
        StringBuilder result = new StringBuilder();
        GeneticData? data1 = null, data2 = null;
        List<string> missingProteins = new List<string>();
        foreach (GeneticData data in geneticDataList)
        {
            if (data.protein.Equals(protein1, StringComparison.OrdinalIgnoreCase))
                data1 = data;
            if (data.protein.Equals(protein2, StringComparison.OrdinalIgnoreCase))
                data2 = data;
        }

        if (data1 == null) missingProteins.Add(protein1);
        if (data2 == null) missingProteins.Add(protein2);

        if (missingProteins.Count > 0)
        {
            result.AppendLine("amino-acids difference:");
            result.Append($"MISSING: {string.Join(", ", missingProteins)}");
        }
        else
        {

            // Calculate differences
            string seq1 = data1.Value.amino_acids;
            string seq2 = data2.Value.amino_acids;
            int differences = 0;
            int minLength = Math.Min(seq1.Length, seq2.Length);

            for (int i = 0; i < minLength; i++)
            {
                if (seq1[i] != seq2[i])
                    differences++;
            }

            differences += Math.Abs(seq1.Length - seq2.Length);

            result.AppendLine("amino-acids difference:");
            result.Append(differences.ToString());
        }
        return result.ToString();
    }

    //Find most frequent amino acid in a protein
    static string ModeProtein(string proteinName)
    {
        StringBuilder result = new StringBuilder();
        GeneticData? proteinData = null;
        foreach (GeneticData data in geneticDataList)
        {
            if (data.protein.Equals(proteinName, StringComparison.OrdinalIgnoreCase))
            {
                proteinData = data;
                break;
            }
        }

        if (proteinData == null)
        {
            result.AppendLine("amino-acid occurs:");
            result.Append($"MISSING: {proteinName}");
        }
        else
        {
            // Calculate frequency of each amino acid
            Dictionary<char, int> frequency = new Dictionary<char, int>();
            foreach (char c in proteinData.Value.amino_acids)
            {
                if (frequency.ContainsKey(c))
                    frequency[c]++;
                else
                    frequency[c] = 1;
            }

            // Find the most frequent amino acid
            char mostFrequent = ' ';
            int maxCount = 0;

            foreach (var pair in frequency)
            {
                if (pair.Value > maxCount ||
                    (pair.Value == maxCount && pair.Key < mostFrequent))
                {
                    mostFrequent = pair.Key;
                    maxCount = pair.Value;
                }
            }

            result.AppendLine("amino-acid occurs:");
            result.Append($"{mostFrequent} {maxCount}");
        }
        return result.ToString();
    }

    static string RLEncoding(string amino_acids)
    {
        StringBuilder encoded = new StringBuilder();
        if (string.IsNullOrEmpty(amino_acids))
            return "";


        int count = 1;
        char current = amino_acids[0];

        for (int i = 1; i < amino_acids.Length; i++)
        {
            if (amino_acids[i] == current && count < 9) 
            {
                count++;
            }
            else
            {
                // Only add count if it's greater than 2 (saves space)
                if (count > 2)
                {
                    encoded.Append(count);
                }
                else if (count == 2)
                {
                    encoded.Append(current);
                }
                encoded.Append(current);

                current = amino_acids[i];
                count = 1;
            }
        }

        // Append the last character(s)
        if (count > 2)/////
        {
            encoded.Append(count);
        }
        else if (count == 2)
        {
            encoded.Append(current);
        }
        encoded.Append(current);

        return encoded.ToString();
    }

    static string RLDecoding(string amino_acids)
    {
        StringBuilder decoded = new StringBuilder();
        int i = 0;

        while (i < amino_acids.Length)
        {
            if (char.IsDigit(amino_acids[i]))
            {
                // Get the count using int.Parse()
                int count = int.Parse(amino_acids[i].ToString());
                char aminoAcid = amino_acids[i + 1];

                // Append the character 'count' times
                decoded.Append(aminoAcid, count);

                // Move past the digit and the character
                i += 2;
            }
            else
            {
                // Single character
                decoded.Append(amino_acids[i]);
                i++;
            }
        }

        return decoded.ToString();
    }
}