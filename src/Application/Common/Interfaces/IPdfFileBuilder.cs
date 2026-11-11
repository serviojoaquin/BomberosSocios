namespace BaseApp.Application.Common.Interfaces;

public interface IPdfFileBuilder
{
    byte[] Merge(IEnumerable<byte[]> pdfs);
}
