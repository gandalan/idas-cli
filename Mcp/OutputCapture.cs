namespace IdasCli.Mcp;

/// <summary>
/// Captures console output for interception - thread-safe version
/// Uses a global lock to ensure only one capture is active at a time
/// </summary>
public class OutputCapture : IDisposable
{
    private static readonly object _globalLock = new();
    private static TextWriter? _originalOutput;
    private static int _activeCaptures = 0;
    private readonly StringWriter _stringWriter;
    private bool _disposed;

    public OutputCapture()
    {
        _stringWriter = new StringWriter();
        
        lock (_globalLock)
        {
            if (_activeCaptures == 0)
            {
                _originalOutput = Console.Out;
            }
            _activeCaptures++;
            Console.SetOut(_stringWriter);
        }
    }

    public string GetCapturedOutput()
    {
        lock (_globalLock)
        {
            return _stringWriter.ToString();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        lock (_globalLock)
        {
            _activeCaptures--;
            if (_activeCaptures == 0 && _originalOutput != null)
            {
                Console.SetOut(_originalOutput);
            }
            // Don't restore Console.Out if there are still active captures
            // The next active capture's StringWriter is already set
        }
        
        _stringWriter.Dispose();
        _disposed = true;
    }
}
