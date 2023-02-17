using System;
using System.IO;
using System.Text;


/// <summary>
/// Represents a writer for CSV data.
/// </summary>
public class CSVWriter : IDisposable {

    private readonly char[] specialChars = new[] { '"', '\0' };
    private Stream stream;
    private char terminator;
    private StreamWriter writer;



    /// <summary>
    /// Initializes a new <see cref="CSVWriter"/>.
    /// </summary>
    /// <param name="path">The path of the CSV file to write to.</param>
    /// <param name="terminator">Optional. The terminator to use between CSV columns.</param>
    public CSVWriter(string path, char terminator = ',') {
        if ( string.IsNullOrEmpty(path) ) throw new ArgumentNullException(nameof(path));
        if ( terminator == '\0' ) throw new ArgumentNullException(nameof(terminator));

        var directory = Path.GetDirectoryName(path);
        Directory.CreateDirectory(directory);

        this.stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
        this.writer = new StreamWriter(this.stream);
        this.terminator = terminator;

        this.specialChars[1] = terminator;
    }

    /// <summary>
    /// Finalizes the instance.
    /// </summary>
    ~CSVWriter() => this.Dispose(false);


    /// <summary>
    /// Disposes all resources used by the instance.
    /// </summary>
    public void Dispose() {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes all resources used by the instance.
    /// </summary>
    /// <param name="disposing">A flag indicating whether <see cref="Dispose"/> has been called.</param>
    protected virtual void Dispose(bool disposing) {
        this.writer?.Flush();
        this.stream?.Dispose();
        this.stream = null;
        this.writer = null;
    }



    /// <summary>
    /// Writes a row to the CSV stream.
    /// </summary>
    /// <param name="values">Params. The columns to write to the next row.</param>
    public void WriteRow(params object[] values) {
        var rowBuilder = new StringBuilder();
        var isFirstColumn = true;

        foreach ( var value in values ) {
            if ( !isFirstColumn ) {
                rowBuilder.Append(this.terminator);
            }

            if ( !( value is null ) ) {
                var cell = value.ToLogString();
                if ( cell.IndexOfAny(this.specialChars) != -1 ) {
                    rowBuilder.Append($"\"{cell.Replace("\"", "\"\"")}\"");
                }
                else {
                    rowBuilder.Append(cell);
                }
            }

            isFirstColumn = false;
        }

        var row = rowBuilder.ToString();
        this.writer.WriteLine(row);
    }

}