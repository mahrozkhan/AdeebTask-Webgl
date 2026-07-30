using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace AdeebTask.Commands
{
    public class CommandExecutor
    {
        private readonly Stack<ICommand> _history = new Stack<ICommand>();
        private readonly Stack<ICommand> _redoStack = new Stack<ICommand>();
        private bool _isExecuting = false;

        public async UniTask ExecuteAsync(ICommand command)
        {
            if (_isExecuting) return; 
            _isExecuting = true;

            try 
            {
                await command.ExecuteAsync();
                _history.Push(command);
                _redoStack.Clear(); 
            }
            finally
            {
                _isExecuting = false;
            }
        }

        public async UniTask UndoAsync()
        {
            if (_isExecuting || _history.Count == 0) return;
            _isExecuting = true;

            try
            {
                var cmd = _history.Pop();
                await cmd.UndoAsync();
                _redoStack.Push(cmd);
            }
            finally
            {
                _isExecuting = false;
            }
        }

        public async UniTask RedoAsync()
        {
            if (_isExecuting || _redoStack.Count == 0) return;
            _isExecuting = true;

            try
            {
                var cmd = _redoStack.Pop();
                await cmd.ExecuteAsync();
                _history.Push(cmd);
            }
            finally
            {
                _isExecuting = false;
            }
        }
    }
}
