namespace CenterWindow.Contracts.Services;

/// <summary>
/// Provides functionality for capturing the handle of the window currently located under the cursor.
/// </summary>
/// <remarks>This service enables asynchronous retrieval of the handle of the window under the cursor at the time
/// of invocation. It is useful for scenarios where identifying or interacting with the window under the user's pointer
/// is required.</remarks>
public interface IMouseHookService
{
    Task<IntPtr> CaptureMouse(bool capture, CancellationToken cancellationToken = default);

    /// <summary>
    /// Captures the handle of the window currently located under the cursor.
    /// </summary>
    /// <remarks>This method asynchronously retrieves the handle of the window under the cursor at the time of
    /// invocation. It can be used to identify or interact with the window that the user is pointing to.</remarks>
    /// <param name="cancellationToken">A token that can be used to cancel the operation. If the operation is canceled, the returned task will be in a
    /// canceled state.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the handle of the window under the
    /// cursor. If no window is found, the result may be <see cref="IntPtr.Zero"/>.</returns>
    Task<IntPtr> CaptureWindowUnderCursorAsync(CancellationToken cancellationToken = default);
} //