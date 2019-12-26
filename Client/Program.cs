using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;

namespace Tetris
{
    class Block
    {
        public bool isEmpty = false;
        public bool isMoving = false;
        public ConsoleColor color = ConsoleColor.White;
    }
    class BlockForForm
    {
        public bool isEmpty = false;
        public bool isMoving = false;
        public string color = "";
    }

    class BlockTemplate
    {
        public bool[,] isBlocked = new bool[4, 4];
        public int width, height;
        public float cenx, ceny;
        public BlockTemplate(string s)
        {
            int miny = 4, maxy = 0, minx = 4, maxx = 0;
            int i = 0;
            for (int y = 0; y < 4; ++y)
            {
                for (int x = 0; x  < 4; ++x)
                {
                    if (s[i] == '■')
                    {
                        isBlocked[x, y] = true;
                        if (y > maxy) maxy = y;
                        if (y < miny) miny = y;
                        if (x > maxx) maxx = x;
                        if (x < minx) minx = x;
                    }
                    else isBlocked[x, y] = false;
                    ++i;
                }
            }
            height = maxy - miny + 1;
            width = maxx - minx + 1;
            ceny = (maxy + miny) / 2f + 0.5f;
            cenx = (maxx + minx) / 2f + 0.5f;
        }
    }

    class Point
    {
        public int x;
        public int y;
    }

    class Game
    {   
        public static void Shuffle<T>(Random rand, IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rand.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        Random rand = new Random();

        const int board_w = 10;
        const int board_h = 20;
        const int hidden_board_h = 4;
        const int board_border = 1;
        public bool Run = true;

        int now_x;
        int now_y;
        string now_block;
        string hold_block = "";
        int now_angle;
        int score = 0;
        int[] score_n_line = { 40, 60, 200, 900 };

        Block[,] map = new Block[board_w, board_h + hidden_board_h];
        Dictionary<string, Tuple<List<BlockTemplate>, ConsoleColor>> block_template = new Dictionary<string, Tuple<List<BlockTemplate>, ConsoleColor>>();
        List<string> queue = new List<string>();
        List<Point> ghost = new List<Point>();
        int ghost_dy;
        ConsoleColor ghost_color;
        Stopwatch sw = new Stopwatch();
        long drop_ticks = 0;
        public Game()
        {
            {
                List<BlockTemplate> list = new List<BlockTemplate>();
                list.Add(new BlockTemplate(
                    "--■-" +
                    "--■-" +
                    "--■-" +
                    "--■-"));
                list.Add(new BlockTemplate(
                    "----" +
                    "----" +
                    "■■■■" +
                    "----"));
                block_template["I"] = new Tuple<List<BlockTemplate>, ConsoleColor>(list, ConsoleColor.Cyan);
            }
            {
                List<BlockTemplate> list = new List<BlockTemplate>();
                list.Add(new BlockTemplate(
                    "----" +
                    "-■■-" +
                    "-■■-" +
                    "----"));
                block_template["O"] = new Tuple<List<BlockTemplate>, ConsoleColor>(list, ConsoleColor.Yellow);
            }
            {
                List<BlockTemplate> list = new List<BlockTemplate>();
                list.Add(new BlockTemplate(
                    "----" +
                    "■■■-" +
                    "--■-" +
                    "----"));
                list.Add(new BlockTemplate(
                    "-■--" +
                    "-■--" +
                    "■■--" +
                    "----"));
                list.Add(new BlockTemplate(
                    "■---" +
                    "■■■-" +
                    "----" +
                    "----"));
                list.Add(new BlockTemplate(
                    "-■■-" +
                    "-■--" +
                    "-■--" +
                    "----"));
                block_template["J"] = new Tuple<List<BlockTemplate>, ConsoleColor>(list, ConsoleColor.Blue);
            }
            {
                List<BlockTemplate> list = new List<BlockTemplate>();
                list.Add(new BlockTemplate(
                    "----" +
                    "■■■-" +
                    "■---" +
                    "----"));
                list.Add(new BlockTemplate(
                    "■■--" +
                    "-■--" +
                    "-■--" +
                    "----"));
                list.Add(new BlockTemplate(
                    "--■-" +
                    "■■■-" +
                    "----" +
                    "----"));
                list.Add(new BlockTemplate(
                    "-■--" +
                    "-■--" +
                    "-■■-" +
                    "----"));
                block_template["L"] = new Tuple<List<BlockTemplate>, ConsoleColor>(list, ConsoleColor.DarkYellow);
            }
            {
                List<BlockTemplate> list = new List<BlockTemplate>();
                list.Add(new BlockTemplate(
                    "-■■-" +
                    "■■--" +
                    "----" +
                    "----"));
                list.Add(new BlockTemplate(
                    "-■--" +
                    "-■■-" +
                    "--■-" +
                    "----"));
                list.Add(new BlockTemplate(
                    "----" +
                    "-■■-" +
                    "■■--" +
                    "----"));
                list.Add(new BlockTemplate(
                    "-■--" +
                    "-■■-" +
                    "--■-" +
                    "----"));
                block_template["S"] = new Tuple<List<BlockTemplate>, ConsoleColor>(list, ConsoleColor.Green);
            }
            {
                List<BlockTemplate> list = new List<BlockTemplate>();
                list.Add(new BlockTemplate(
                    "----" +
                    "■■■-" +
                    "-■--" +
                    "----"));
                list.Add(new BlockTemplate(
                    "-■--" +
                    "■■--" +
                    "-■--" +
                    "----"));
                list.Add(new BlockTemplate(
                    "-■--" +
                    "■■■-" +
                    "----" +
                    "----"));
                list.Add(new BlockTemplate(
                    "-■--" +
                    "-■■-" +
                    "-■--" +
                    "----"));
                block_template["T"] = new Tuple<List<BlockTemplate>, ConsoleColor>(list, ConsoleColor.DarkMagenta);
            }
            {
                List<BlockTemplate> list = new List<BlockTemplate>();
                list.Add(new BlockTemplate(
                    "----" +
                    "■■--" +
                    "-■■-" +
                    "----"));
                list.Add(new BlockTemplate(
                    "--■-" +
                    "-■■-" +
                    "-■--" +
                    "----"));
                list.Add(new BlockTemplate(
                    "----" +
                    "■■--" +
                    "-■■-" +
                    "----"));
                list.Add(new BlockTemplate(
                    "--■-" +
                    "-■■-" +
                    "-■--" +
                    "----"));
                block_template["Z"] = new Tuple<List<BlockTemplate>, ConsoleColor>(list, ConsoleColor.Red);
            }

            for (int i = 0; i < 3; ++i)
            {
                FillQueue();
            }
            DropNewBlock();
            sw.Start();
        }
        public void Draw()
        {
            Console.CursorVisible = false;
            Console.BufferWidth = 100;
            Console.BufferHeight = 100;

            Console.SetCursorPosition(0, 0);
            Console.ForegroundColor = ConsoleColor.White;
            for (int i = 0; i < board_border; ++i)
            {
                for (int x = 0; x < board_w + board_border * 2; ++x) Console.Write('□');
            }
            Console.Write('\n');
            for (int y = 0; y < board_h; ++y)
            {
                Console.ForegroundColor = ConsoleColor.White;
                for (int i = 0; i < board_border; ++i) Console.Write('□');
                for (int x = 0; x < board_w; ++x)
                {
                    if (map[x, y + hidden_board_h] != null)
                    {
                        Console.ForegroundColor = map[x, y + hidden_board_h].color;
                        if (map[x, y + hidden_board_h].isMoving) Console.BackgroundColor = ConsoleColor.DarkGray;
                        Console.Write('■');
                        Console.BackgroundColor = ConsoleColor.Black;
                    }
                    else
                    {
                        bool contain = false;
                        foreach (Point p in ghost)
                        {
                            if (x == p.x && y + hidden_board_h == p.y)
                            {
                                contain = true;
                                break;
                            }
                        }
                        if (contain)
                        {
                            Console.ForegroundColor = ghost_color;
                            Console.Write('▒');
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        else Console.Write('　');
                    }

                }
                Console.ForegroundColor = ConsoleColor.White;
                for (int i = 0; i < board_border; ++i) Console.Write('□');
                Console.Write('\n');
            }
            for (int i = 0; i < board_border; ++i)
            {
                for (int x = 0; x < board_w + board_border * 2; ++x) Console.Write('□');
            }
            Console.Write('\n');

            Console.SetCursorPosition((board_w + board_border * 2) * 2, 0);
            Console.Write("□□HOLD□□□");
            Console.SetCursorPosition((board_w + board_border * 2) * 2, 1);
            Console.Write("            □");
            for (int y = 0; y < 4; ++y)
            {
                Console.SetCursorPosition((board_w + board_border * 2) * 2, y + 2);
                Console.Write("  ");
                if (hold_block != "")
                {
                    for (int x = 0; x < 4; ++x)
                    {
                        if (block_template[hold_block].Item1[0].isBlocked[x, y])
                        {
                            Console.ForegroundColor = block_template[hold_block].Item2;
                            Console.Write("■");
                        }
                        else
                            Console.Write("  ");
                    }
                }
                else
                {
                    for (int x = 0; x < 4; ++x) Console.Write("  ");
                }
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("  □");
            }
            Console.SetCursorPosition((board_w + board_border * 2) * 2, 6);
            Console.Write("            □");
            Console.SetCursorPosition((board_w + board_border * 2) * 2, 7);
            for (int x = 0; x < 7; ++x) Console.Write('□');
            Console.SetCursorPosition((board_w + board_border * 2) * 2, 8);
            Console.Write("            □");
            for (int y = 0; y < 4; ++y)
            {
                Console.SetCursorPosition((board_w + board_border * 2) * 2, y + 9);
                Console.Write("  ");
                for (int x = 0; x < 4; ++x)
                {
                    if (block_template[queue[0]].Item1[0].isBlocked[x, y])
                    {
                        Console.ForegroundColor = block_template[queue[0]].Item2;
                        Console.Write("■");
                    }
                    else
                        Console.Write("  ");
                }
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("  □");
            }
            Console.SetCursorPosition((board_w + board_border * 2) * 2, 13);
            Console.Write("            □");
            Console.SetCursorPosition((board_w + board_border * 2) * 2, 14);
            for (int x = 0; x < 7; ++x) Console.Write('□');
            Console.SetCursorPosition((board_w + board_border * 2) * 2, 16);
            Console.WriteLine("  Score: " + score.ToString());
        }
        
        void FillQueue()
        {
            List<string> temp_queue = new List<string>();
            foreach (string key in block_template.Keys)
            {
                temp_queue.Add(key);
            }
            Shuffle<string>(rand, temp_queue);
            foreach (string s in temp_queue)
            {
                queue.Add(s);
            }
        }
        void DropNewBlock()
        {
            now_block = queue[0];
            queue.RemoveAt(0);
            if (queue.Count < block_template.Count * 2)
                FillQueue();
            now_x = (board_w - 4) / 2;
            now_y = 0;
            now_angle = 0;
            FillTemplate(now_block, now_angle, now_x, now_y);
            RefreshGhost();
        }

        void RefreshGhost()
        {
            ghost.Clear();
            ghost_color = block_template[now_block].Item2;
            EraseTemplate(now_block, now_angle, now_x, now_y);
            int dy = 0;
            do
            {
                CheckTemplateResult res = CheckTemplate(now_block, now_angle, now_x, now_y + dy);
                if (res == CheckTemplateResult.OK) ++dy;
                else break;
            } while (true);
            if (dy > 1)
            {
                dy -= 1;
                ghost_dy = dy;
                var color = block_template[now_block].Item2;
                var temp = block_template[now_block].Item1[now_angle];
                for (int x = 0; x < 4; ++x)
                {
                    for (int y = 0; y < 4; ++y)
                    {
                        if (temp.isBlocked[x, y]) ghost.Add(new Point { x = x + now_x, y = y + now_y + dy });
                    }
                }
            }
            FillTemplate(now_block, now_angle, now_x, now_y, true);
        }

        void FillTemplate(string tmp_name, int index, int dx, int dy, bool moving = false)
        {
            var color = block_template[tmp_name].Item2;
            var temp = block_template[tmp_name].Item1[index];
            for (int x = 0; x < 4; ++x)
            {
                for (int y = 0; y < 4; ++y)
                {
                    if (temp.isBlocked[x, y])
                        map[x + dx, y + dy] = new Block { isEmpty = false, color = color, isMoving = moving };
                }
            }
        }
        void EraseTemplate(string tmp_name, int index, int dx, int dy)
        {
            var temp = block_template[tmp_name].Item1[index];
            for (int x = 0; x < 4; ++x)
            {
                for (int y = 0; y < 4; ++y)
                {
                    if (temp.isBlocked[x, y])
                        map[x + dx, y + dy] = null;
                }
            }
        }

        enum CheckTemplateResult
        {
            OK, Blocked, Out
        }
        CheckTemplateResult CheckTemplate(string tmp_name, int index, int dx, int dy)
        {
            CheckTemplateResult result = CheckTemplateResult.OK;
            var temp = block_template[tmp_name].Item1[index];
            for (int x = 0; x < 4; ++x)
            {
                for (int y = 0; y < 4; ++y)
                {
                    if (temp.isBlocked[x, y])
                    {
                        if ((x + dx) >= 0 && (x + dx) < board_w && (y + dy) >= 0 && (y + dy) < board_h + hidden_board_h)
                        {
                            if (map[x + dx, y + dy] != null)
                            {
                                result = CheckTemplateResult.Blocked;
                            }
                        }
                        else
                        {
                            return CheckTemplateResult.Out;
                        }
                    }
                }
            }
            return result;
        }

        

        public void Loop()
        {
            sw.Stop();
            drop_ticks += sw.ElapsedMilliseconds;
            sw.Reset();
            if (Console.KeyAvailable)
            {
                ConsoleKey comm = Console.ReadKey().Key;
                switch (comm)
                {
                    case ConsoleKey.Spacebar:
                        {
                            for (int y = 0; y <= ghost_dy; ++y)
                            {
                                EraseTemplate(now_block, now_angle, now_x, now_y);
                                CheckTemplateResult res = CheckTemplate(now_block, now_angle, now_x, now_y + 1);
                                if (res == CheckTemplateResult.OK) now_y += 1;
                                if (res != CheckTemplateResult.OK)
                                {
                                    FillTemplate(now_block, now_angle, now_x, now_y);
                                    now_block = null;
                                    drop_ticks = 0;
                                    break;
                                }
                                else FillTemplate(now_block, now_angle, now_x, now_y, true);
                            }
                        }
                        break;
                    case ConsoleKey.DownArrow:
                        {
                            EraseTemplate(now_block, now_angle, now_x, now_y);
                            CheckTemplateResult res = CheckTemplate(now_block, now_angle, now_x, now_y + 1);
                            if (res == CheckTemplateResult.OK) now_y += 1;
                            if (res != CheckTemplateResult.OK)
                            {
                                FillTemplate(now_block, now_angle, now_x, now_y);
                                now_block = null;
                                drop_ticks = 0;
                            }
                            else FillTemplate(now_block, now_angle, now_x, now_y, true);
                        }
                        break;
                    case ConsoleKey.LeftArrow:
                        {
                            EraseTemplate(now_block, now_angle, now_x, now_y);
                            CheckTemplateResult res = CheckTemplate(now_block, now_angle, now_x - 1, now_y);
                            if (res == CheckTemplateResult.OK) now_x -= 1;
                            FillTemplate(now_block, now_angle, now_x, now_y, true);

                            RefreshGhost();
                        }
                        break;
                    case ConsoleKey.RightArrow:
                        {
                            EraseTemplate(now_block, now_angle, now_x, now_y);
                            CheckTemplateResult res = CheckTemplate(now_block, now_angle, now_x + 1, now_y);
                            if (res == CheckTemplateResult.OK) now_x += 1;
                            FillTemplate(now_block, now_angle, now_x, now_y, true);

                            RefreshGhost();
                        }
                        break;
                    case ConsoleKey.UpArrow:
                        {
                            EraseTemplate(now_block, now_angle, now_x, now_y);

                            CheckTemplateResult res = CheckTemplate(now_block, (now_angle + 1) % block_template[now_block].Item1.Count, now_x, now_y);
                            if (res == CheckTemplateResult.OK) now_angle = (now_angle + 1) % block_template[now_block].Item1.Count;
                            FillTemplate(now_block, now_angle, now_x, now_y, true);

                            RefreshGhost();
                        }
                        break;
                    case ConsoleKey.End:
                        {
                            //if (now_y < hidden_board_h)
                            {
                                if (hold_block == "")
                                {
                                    hold_block = now_block;
                                    EraseTemplate(now_block, now_angle, now_x, now_y);
                                    now_block = null;
                                }
                                else
                                {
                                    string s = now_block;
                                    EraseTemplate(now_block, now_angle, now_x, now_y);
                                    now_block = hold_block;
                                    hold_block = s;
                                    now_x = (board_w - 4) / 2;
                                    now_y = 0;
                                    now_angle = 0;
                                    RefreshGhost();
                                }
                            }
                        }
                        break;
                }
            }
            if (drop_ticks > 500 && now_block != null)
            {
                drop_ticks = 0;
                EraseTemplate(now_block, now_angle, now_x, now_y);
                CheckTemplateResult res = CheckTemplate(now_block, now_angle, now_x, now_y + 1);
                if (res == CheckTemplateResult.OK) now_y += 1;
                if (res != CheckTemplateResult.OK)
                {
                    FillTemplate(now_block, now_angle, now_x, now_y);
                    now_block = null;
                }
                else FillTemplate(now_block, now_angle, now_x, now_y, true);
            }

            if (now_block == null)
            {
                DropNewBlock();
            }

            int combo = 0;
            for (int y = 0; y < board_h; ++y)
            {
                int block_num = 0;
                for (int x = 0; x < board_w; ++x)
                {
                    if (map[x, y + hidden_board_h] != null)
                    {
                        if (!(map[x, y + hidden_board_h].isMoving || map[x, y + hidden_board_h].isEmpty)) ++block_num;
                    }
                }
                if (block_num == board_w)
                {
                    Draw();
                    Thread.Sleep(150);
                    for (int A = 0; A < board_w + 1; ++A)
                    {
                        Console.SetCursorPosition(board_border * 2, board_border + y);
                        for (int x = 0; x < A; ++x)
                        {
                            Console.ForegroundColor = map[x, y + hidden_board_h].color;
                            Console.Write("▣");
                        }
                        Thread.Sleep(50);
                    }
                    Thread.Sleep(50);
                    Console.SetCursorPosition(board_border * 2, board_border + y);
                    for (int x = 0; x < board_w; ++x)
                    {
                        Console.ForegroundColor = map[x, y + hidden_board_h].color;
                        Console.Write("▦");
                    }
                    Thread.Sleep(200);

                    for (int _y = y; _y > 1; --_y)
                    {
                        for (int x = 0; x < board_w; ++x)
                        {
                            map[x, _y + hidden_board_h] = map[x, _y - 1 + hidden_board_h];
                            map[x, _y - 1 + hidden_board_h] = null;
                        }
                    }
                    Draw();
                    Thread.Sleep(300);
                    RefreshGhost();
                    score += score_n_line[combo];
                    ++combo;
                }
            }
            sw.Start();
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Form form = new FormGame();
            form.ShowDialog();
        }
    }
}
