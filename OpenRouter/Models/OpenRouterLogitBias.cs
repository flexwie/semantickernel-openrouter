namespace SemanticKernel.Connectors.OpenRouter.Models;

/// <summary>
/// Helper class for creating logit bias configurations.
/// </summary>
public static class OpenRouterLogitBias
{
    /// <summary>
    /// Creates a logit bias dictionary from token-bias pairs.
    /// </summary>
    /// <param name="biases">Array of (tokenId, bias) tuples where bias is between -100 and 100.</param>
    /// <returns>Dictionary suitable for LogitBias property.</returns>
    public static Dictionary<int, int> Create(params (int tokenId, int bias)[] biases)
    {
        var result = new Dictionary<int, int>();
        foreach (var (tokenId, bias) in biases)
        {
            if (bias < -100 || bias > 100)
            {
                throw new ArgumentException($"Bias value {bias} is out of range. Must be between -100 and 100.", nameof(bias));
            }
            result[tokenId] = bias;
        }
        return result;
    }

    /// <summary>
    /// Creates a logit bias that completely suppresses specific tokens.
    /// </summary>
    /// <param name="tokenIds">Token IDs to suppress.</param>
    /// <returns>Dictionary with maximum negative bias for specified tokens.</returns>
    public static Dictionary<int, int> Suppress(params int[] tokenIds)
    {
        return tokenIds.ToDictionary(tokenId => tokenId, _ => -100);
    }

    /// <summary>
    /// Creates a logit bias that strongly encourages specific tokens.
    /// </summary>
    /// <param name="tokenIds">Token IDs to encourage.</param>
    /// <returns>Dictionary with maximum positive bias for specified tokens.</returns>
    public static Dictionary<int, int> Encourage(params int[] tokenIds)
    {
        return tokenIds.ToDictionary(tokenId => tokenId, _ => 100);
    }

    /// <summary>
    /// Creates a logit bias that reduces the likelihood of specific tokens.
    /// </summary>
    /// <param name="bias">Negative bias value (should be negative, between -100 and 0).</param>
    /// <param name="tokenIds">Token IDs to discourage.</param>
    /// <returns>Dictionary with specified negative bias for tokens.</returns>
    public static Dictionary<int, int> Discourage(int bias, params int[] tokenIds)
    {
        if (bias > 0)
        {
            throw new ArgumentException("Bias should be negative for discouraging tokens.", nameof(bias));
        }
        if (bias < -100)
        {
            throw new ArgumentException("Bias cannot be less than -100.", nameof(bias));
        }
        return tokenIds.ToDictionary(tokenId => tokenId, _ => bias);
    }

    /// <summary>
    /// Creates a logit bias that increases the likelihood of specific tokens.
    /// </summary>
    /// <param name="bias">Positive bias value (should be positive, between 0 and 100).</param>
    /// <param name="tokenIds">Token IDs to favor.</param>
    /// <returns>Dictionary with specified positive bias for tokens.</returns>
    public static Dictionary<int, int> Favor(int bias, params int[] tokenIds)
    {
        if (bias < 0)
        {
            throw new ArgumentException("Bias should be positive for favoring tokens.", nameof(bias));
        }
        if (bias > 100)
        {
            throw new ArgumentException("Bias cannot be greater than 100.", nameof(bias));
        }
        return tokenIds.ToDictionary(tokenId => tokenId, _ => bias);
    }

    /// <summary>
    /// Merges multiple logit bias dictionaries. Later dictionaries override earlier ones for duplicate keys.
    /// </summary>
    /// <param name="biases">Logit bias dictionaries to merge.</param>
    /// <returns>Merged dictionary.</returns>
    public static Dictionary<int, int> Merge(params Dictionary<int, int>[] biases)
    {
        var result = new Dictionary<int, int>();
        foreach (var bias in biases)
        {
            foreach (var kvp in bias)
            {
                result[kvp.Key] = kvp.Value;
            }
        }
        return result;
    }
}