﻿namespace DocumentLab.TextAnalyzer.Strategies.AnalyzeTextStrategies
{
  using Contracts;
  using Contracts.Ocr;
  using Contracts.Extensions;
  using Core.Extensions;
  using TextAnalyzer.Interfaces;
  using System.Linq;
  using System.Collections.Generic;
  using System.Text.RegularExpressions;

  public class AnalyzeAmountsStrategy : IAnalyzeTextStrategy
  {
    public IEnumerable<AnalyzedText> Analyze(OcrResult ocrResult)
    {
      var continuousText = ocrResult.AsContinuousText().ReplaceFaultyCharacters(Constants.NumericAnalysisOcrFixDictionary);

      var amounts = Constants.TextAnalysisConfiguration.AmountRegexes
        .Select(x => Regex.Matches(continuousText, x))
        .SelectMany(y => y.Cast<Match>()
        .Select(x => x.Value));

      amounts = amounts.Select(x => Regex.Replace(x, Constants.TextAnalysisConfiguration.AmountIgnoreRegex, string.Empty));

      amounts = amounts.Select(x =>
      {
        if ((x.Count(c => c == '.') > 0 && x.Count(c => c == ',') > 0) || x.Count(c => c == '.') > 1 || x.Count(c => c == ',') > 1)
        {
          Regex regex = new Regex("(,|\\.)");
          return regex.Replace(x, string.Empty, 1);
        }

        return x;
      });

      amounts = amounts.Replace(",", ".");

      return amounts.Select(x => new AnalyzedText()
      {
        Text = x,
        TextType = TextType.Amount.ToString(),
        BoundingBox = ocrResult.BoundingBox
      })
      .Distinct();
    }
  }
}