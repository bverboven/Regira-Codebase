namespace Regira.Office.Word.Abstractions;

public interface IWordManager : IWordCreator, IWordConverter, IWordMerger, IWordTextExtractor, IWordImageExtractor, IImageRenderer;