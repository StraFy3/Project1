using System;
using System.Collections.Generic;
using System.Text;

// Structure to store genetic data
struct GeneticData
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
            geneticDataList = FileHandler.LoadGeneticData("sequences.txt");
            Console.WriteLine($"Loaded {geneticDataList.Count} valid protein records.");

            // Load commands from file
            string[] commands = FileHandler.LoadCommands("commands.txt");
            Console.WriteLine($"Loaded {commands.Length} commands.");

            // Process commands and generate output
            string output = ProcessCommands(commands);

            // Write results to file
            FileHandler.WriteResults("results.txt", output);

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

    static void EncodeSequence()
    {
        Console.Write("Enter the amino acid sequence to encode: ");
        string input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("Invalid input. Sequence cannot be empty.");
            return;
        }

        if (!IsValidAminoAcidSequence(input))
        {
            Console.WriteLine("Warning: Sequence contains invalid amino acid characters.");
            Console.WriteLine("Valid amino acids are: " + validAminoAcids);
            Console.Write("Do you want to continue anyway? (y/n): ");
            if (Console.ReadLine().ToLower() != "y")
                return;
        }

        string encoded = RLEncoding(input);
        Console.WriteLine($"Encoded sequence: {encoded}");
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
    static void SearchSequence()
    {
        if (geneticDataList.Count == 0)
        {
            Console.WriteLine("No genetic data loaded. Cannot perform search.");
            return;
        }

        Console.Write("Enter the amino acid sequence to search for: ");
        string searchSequence = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(searchSequence))
        {
            Console.WriteLine("Invalid input. Search sequence cannot be empty.");
            return;
        }

        // Decode if it's RLE encoded
        if (ContainsDigits(searchSequence))
        {
            searchSequence = RLDecoding(searchSequence);
            Console.WriteLine($"Decoded search sequence: {searchSequence}");
        }

        bool found = false;
        Console.WriteLine("\nSearch results:");
        Console.WriteLine("===============");

        foreach (GeneticData data in geneticDataList)
        {
            if (data.amino_acids.Contains(searchSequence))
            {
                Console.WriteLine($"Found in: {data.organism} - {data.protein}");
                found = true;
            }
        }

        if (!found)
        {
            Console.WriteLine("NOT FOUND");
        }
    }

    // DIFF function: Compare two proteins
    static void DiffProteins()
    {
        if (geneticDataList.Count == 0)
        {
            Console.WriteLine("No genetic data loaded. Cannot perform diff.");
            return;
        }

        Console.Write("Enter first protein name: ");
        string protein1 = Console.ReadLine();

        Console.Write("Enter second protein name: ");
        string protein2 = Console.ReadLine();

        GeneticData? data1 = null, data2 = null;
        List<string> missingProteins = new List<string>();
      //  char.IsUpper('a');
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
            Console.WriteLine($"amino-acids difference:");
            Console.WriteLine($"MISSING: {string.Join(", ", missingProteins)}");
            return;
        }

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

        Console.WriteLine($"amino-acids difference:");
        Console.WriteLine(differences);
    }

    //Find most frequent amino acid in a protein
    static void ModeProtein()
    {
        if (geneticDataList.Count == 0)
        {
            Console.WriteLine("No genetic data loaded. Cannot perform mode.");
            return;
        }

        Console.Write("Enter protein name: ");
        string proteinName = Console.ReadLine();

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
            Console.WriteLine($"amino-acid occurs:");
            Console.WriteLine($"MISSING: {proteinName}");
            return;
        }

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

        Console.WriteLine($"amino-acid occurs:");
        Console.WriteLine($"{mostFrequent} {maxCount}");
    }

    // Check if amino acid sequence is valid
    static void ValidateSequence()
    {
        Console.Write("Enter the amino acid sequence to validate: ");
        string input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("Invalid input. Sequence cannot be empty.");
            return;
        }

        if (IsValidAminoAcidSequence(input))
        {
            Console.WriteLine("Sequence is VALID - contains only standard amino acid codes.");
        }
        else
        {
            Console.WriteLine("Sequence is INVALID - contains non-standard characters.");
            Console.WriteLine("Valid amino acids are: " + validAminoAcids);

            // Show which characters are invalid
            List<char> invalidChars = new List<char>();
            foreach (char c in input)
            {
                if (validAminoAcids.IndexOf(char.ToUpper(c)) == -1 && !invalidChars.Contains(c))
                    invalidChars.Add(c);
            }

            if (invalidChars.Count > 0)
                Console.WriteLine("Invalid characters: " + string.Join(", ", invalidChars));
        }
    }

    // Check if a sequence contains only valid amino acid characters
    static bool IsValidAminoAcidSequence(string sequence)
    {
        foreach (char c in sequence)
        {
            if (validAminoAcids.IndexOf(char.ToUpper(c)) == -1)
                return false;
        }
        return true;
    }

    // Check if a string contains digits (to detect RLE encoding)
    static bool ContainsDigits(string s)
    {
        foreach (char c in s)
        {
            if (char.IsDigit(c))
                return true;
        }
        return false;
    }

    static string RLEncoding(string amino_acids)
    {
        StringBuilder encoded = new StringBuilder();
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