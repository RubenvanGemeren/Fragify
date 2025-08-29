using FragifyTracker.Models;

namespace FragifyTracker.UI;

public interface IUserInterface
{
    void UpdateDisplay(GameStats? stats);
    void HandleInput(ConsoleKeyInfo? key = null);
    void Initialize();
    void Shutdown();
    bool IsRunning { get; }
}
