using System;
using System.Threading;

namespace ConsoleExtractAddress
{
    public class ConsoleSpinner : IDisposable
    {
        private const string Sequence = @"/-\|";
        //private const string Sequence = @".o0o";
        //private const string Sequence = @"+x";
        private int counter = 0;
        private readonly int delay;
        private bool active;
        private readonly Thread thread;

        public ConsoleSpinner(int delay = 100)
        {
            this.delay = delay;
            thread = new Thread(Spin);
        }

        public void Start()
        {
            active = true;
            if (!thread.IsAlive)
                thread.Start();
        }

        public void Stop()
        {
            active = false;
            Draw(' ');
        }

        private void Spin()
        {
            while (active)
            {
                Turn();
                Thread.Sleep(delay);
            }
        }

        private void Draw(char c)
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            if (c != ' ')
            {
                Console.Write(c);
            }
        }

        private void Turn()
        {
            Draw(Sequence[++counter % Sequence.Length]);
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
