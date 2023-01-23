using Microsoft.AspNetCore.SignalR;

namespace Training.CircleSquareGame.Api;

public class XOHub : Hub
{
    public GameState _gameState;
    const int _gridSize = 3;
    public XOHub()
    {
        ResetGameState();
    }

    private void ResetGameState()
    {
        _gameState = new();
        
        for (int x = 0; x < _gridSize; x++)
        {
            for (int y = 0; y < _gridSize; y++)
            {
                _gameState.Fields.Add(new Field { X = x, Y = y });
            }
        }
        _gameState.CurrentPlayer = Player.O;
        _gameState.GameResult = GameResult.NotFinishedYet;
    }

    public async Task GetField(string fieldId)
    {
        await Clients.All.SendAsync("CurrentFieldValue", fieldId, "");
    }
    
    public async Task SetField(int x, int y)
    {
        if (_gameState.Fields.FirstOrDefault(f => f.X == x && f.Y == y) == null)
        {
            return;
            // TODO: return error
        }

        if (_gameState.Fields.First(f => f.X == x && f.Y == y).Value == FieldValue.Empty)
        {
            _gameState.Fields.First(f => f.X == x && f.Y == y).Value = _gameState.CurrentPlayer;
        }

        

        _gameState.GameResult = DefineGameResult();
        _gameState.CurrentPlayer = DefineCurrentPlayer();
        // TODO: Send game state
        await Clients.All.SendAsync("CurrentFieldValue", _gameState, "x");
    }

    private string DefineCurrentPlayer()
    {
        if (_gameState.CurrentPlayer == Player.O)
        {
            return Player.X;
        }
        else
        {
           return Player.O;
        }
    }

    private string DefineGameResult()
    {
        var XFields = _gameState.Fields.Where(f => f.Value == Player.X);
        var OFields = _gameState.Fields.Where(f => f.Value == Player.O);

        if (HasThreeInLine(XFields))
        {
            return GameResult.XWon;
        }
        else if (HasThreeInLine(OFields))
        {
            return GameResult.OWon;
        }
        else if (_gameState.Fields.Any(f => f.Value == FieldValue.Empty))
        {
            return GameResult.NotFinishedYet;
        }
        else
        {
            return GameResult.Draw;
        }
    }

    private bool HasThreeInLine(IEnumerable<Field> fields)
    {
        if (HasAllHorizontal(fields) || HasAllVertical(fields) || HasAllDiagonal(fields))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool HasAllDiagonal(IEnumerable<Field> fields)
    {
        for (int i = 0; i < _gridSize; i++)
        {
            if (fields.Where(f => (f.X == i && f.Y == i)).Count() == _gridSize)
            {
                return true;
            }
        }

        for (int i = 0; i < _gridSize; i++)
        {
            if (fields.Where(f => (f.X == i && f.Y == _gridSize - i)).Count() == _gridSize)
            {
                return true;
            }
        }

        return false;
    }

    private bool HasAllVertical(IEnumerable<Field> fields)
    {
        for (int i = 0; i < _gridSize; i++)
        {
            if (fields.Where(f => f.X == i).Count() == _gridSize)
            {
                return true;
            }
        }
        
        return false;
    }

    private bool HasAllHorizontal(IEnumerable<Field> fields)
    {
        for (int i = 0; i < _gridSize; i++)
        {
            if (fields.Where(f => f.Y == i).Count() == _gridSize)
            {
                return true;
            }
        }

        return false;
    }
}

public class Field
{
    public int X { get; set; }
    public int Y { get; set; }
    public string Value { get; set; } = FieldValue.Empty;
}

public static class FieldValue
{
    public const string X = "x";
    public const string O = "o";
    public const string Empty = "";
}

public static class Player
{
    public const string X = "x";
    public const string O = "o";
}

public static class GameResult
{
    public const string NotFinishedYet = "";
    public const string OWon = "o";
    public const string XWon = "x";
    public const string Draw = "draw";
}

public class GameState
{
    public string CurrentPlayer { get; set; } = Player.O;
    public List<Field> Fields { get; set; } = new List<Field>();
    public string? GameResult { get; set; }
}
