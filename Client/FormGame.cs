using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tetris
{
    public partial class FormGame : Form
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
        bool gameover = false;
        bool connection = true;

        int now_x;
        int now_y;
        string now_block;
        string hold_block = "";
        int now_angle;
        int my_score = 0, other_score = 0;
        int[] score_n_line = { 40, 60, 200, 900 };

        BlockForForm[,] map = new BlockForForm[board_w, board_h + hidden_board_h];
        BlockForForm[,] other_map = new BlockForForm[board_w, board_h];
        Dictionary<string, Tuple<List<BlockTemplate>, string>> block_template = new Dictionary<string, Tuple<List<BlockTemplate>, string>>();
        Dictionary<string, Image> block_image = new Dictionary<string, Image>();
        List<string> queue = new List<string>();
        List<Point> ghost = new List<Point>();
        int ghost_dy;
        int drop_ticks = 0;
        bool stop = false;

        Font fontSmall = new Font("210 맨발의청춘L", 15);
        Font fontBig = new Font("210 맨발의청춘L", 23);
        BufferedGraphicsContext ctx;
        BufferedGraphics bg_mygame, bg_othgame;

        Brush ghost_brush = new SolidBrush(Color.FromArgb(80, 255, 255, 255));
        Brush gameover_brush = new SolidBrush(Color.FromArgb(80, 0, 0, 0));
        PointF div_MainGame = new PointF(130, 10);
        PointF div_Hold = new PointF(17, 10);
        PointF div_Score = new PointF(17, 136);
        PointF div_Next = new PointF(389, 10);
        StringFormat sf_center = new StringFormat();

        Thread trd_removeline, trd_connection, trd_insertline;
        ServerConnector sv = ServerConnector.GetInstace();
        Field my_field = new Field();
        string my_uuid = "";
        bool my_data_fresh = false;
        Field other_field = new Field();
        Dictionary<string, int> stack_dict = new Dictionary<string, int>();
        int push_stack = 0;
        int pop_stack = 0;
        public FormGame()
        {
            InitializeComponent();
            sf_center.LineAlignment = StringAlignment.Center;
            sf_center.Alignment = StringAlignment.Center;

            string now_path = "";// @"C:\Users\Korean\source\repos\Tetris\";
            block_image["Cyan"]     = Image.FromFile(now_path + @"Blocks\cyan.bmp");
            block_image["Yellow"]   = Image.FromFile(now_path + @"Blocks\yellow.bmp");
            block_image["Blue"]     = Image.FromFile(now_path + @"Blocks\blue.bmp");
            block_image["Orange"]   = Image.FromFile(now_path + @"Blocks\orange.bmp");
            block_image["Green"]    = Image.FromFile(now_path + @"Blocks\green.bmp");
            block_image["Magenta"]  = Image.FromFile(now_path + @"Blocks\magenta.bmp");
            block_image["Black"]    = Image.FromFile(now_path + @"Blocks\black.bmp");
            block_image["Red"] = Image.FromFile(now_path + @"Blocks\red.bmp");
            block_image["Gray"] = Image.FromFile(now_path + @"Blocks\gray.bmp");

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
                block_template["I"] = new Tuple<List<BlockTemplate>, string>(list, "Cyan");
            }
            {
                List<BlockTemplate> list = new List<BlockTemplate>();
                list.Add(new BlockTemplate(
                    "----" +
                    "-■■-" +
                    "-■■-" +
                    "----"));
                block_template["O"] = new Tuple<List<BlockTemplate>, string>(list, "Yellow");
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
                block_template["J"] = new Tuple<List<BlockTemplate>, string>(list, "Blue");
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
                block_template["L"] = new Tuple<List<BlockTemplate>, string>(list, "Orange");
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
                block_template["S"] = new Tuple<List<BlockTemplate>, string>(list, "Green");
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
                block_template["T"] = new Tuple<List<BlockTemplate>, string>(list, "Magenta");
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
                block_template["Z"] = new Tuple<List<BlockTemplate>, string>(list, "Red");
            }

            for (int i = 0; i < 3; ++i)
            {
                FillQueue();
            }
            DropNewBlock();


            ctx = BufferedGraphicsManager.Current;
            ctx.MaximumBuffer = new Size(this.Width + 1, this.Height + 1);
            
            bg_mygame = ctx.Allocate(my_game.CreateGraphics(), new Rectangle(this.Location, this.Size));
            bg_othgame = ctx.Allocate(otherGame.CreateGraphics(), new Rectangle(this.Location, this.Size));
        }

        void ConnectionControl()
        {
            
            Invoke(new Action(() =>{
                if (!(this.Disposing || this.IsDisposed))
                    this.Text = "Offline (Not start)";
            }));
            
            while (true)
            {
                if (!connection) break;
                try
                {
                    my_uuid = sv.SendRequest("login", "GET").Item2;
                    break;
                }
                catch (Exception e)
                {
                    //MessageBox.Show(e.ToString());
                    Invoke(new Action(() => {
                        if (!(this.Disposing || this.IsDisposed))
                            this.Text = "Offline " + e.ToString();
                    }));
                    Thread.Sleep(2);
                }
            }
            
            Invoke(new Action(() => {
                if (!(this.Disposing || this.IsDisposed))
                    this.Text = "Online Starting";
            }));
            while (true)
            {
                if (!connection) break;
                {
                    if (my_data_fresh)
                    {
                        UpdateMyField();
                        try
                        {
                            var data = sv.SendRequest("sendData", "POST", JsonConvert.SerializeObject(my_field), $"x-id: {my_uuid}");
                            
                            Invoke(new Action(() =>
                            {
                                if (!(this.Disposing || this.IsDisposed))
                                    this.Text = $"Online {data.Item3}ms";
                            }));
                        }
                        catch
                        {
                            Invoke(new Action(() =>
                            {
                                if (!(this.Disposing || this.IsDisposed))
                                    this.Text = "Offline(Failed to SendData)";
                            }));
                        }

                        my_data_fresh = true;
                    }
                    {
                        try
                        {
                            var data = sv.SendRequest("getData", "GET", null, $"x-id: {my_uuid}");

                            if (data.Item1 == System.Net.HttpStatusCode.OK)
                            {
                                other_field = JsonConvert.DeserializeObject<Field>(data.Item2);
                                Invoke(new Action(() =>
                                {
                                    if (!(this.Disposing || this.IsDisposed))
                                        this.Text = $"Online {data.Item3}ms";
                                }));
                            }
                            else
                            {
                                Invoke(new Action(() =>
                                {
                                    if (!(this.Disposing || this.IsDisposed))
                                        this.Text = $"Online Not Found";
                                }));
                            }
                        }
                        catch (Exception e)
                        {
                            //System.Diagnostics.Debug.WriteLine(data.Item2);
                            //System.Diagnostics.Debug.WriteLine(e.ToString());
                            
                            Invoke(new Action(() =>
                            {
                                if (!(this.Disposing || this.IsDisposed))
                                    this.Text = "Offline(Failed to GetData)";
                            }));
                        }
                        Invoke(new Action(() =>
                        {
                            if (!(this.Disposing || this.IsDisposed))
                            {
                                UpdateOtherField();
                                otherGame_Paint(null, null);
                            }
                        }));
                    }
                    {
                        try
                        {
                            var data = sv.SendRequest("getStack", "GET", null, $"x-id: {my_uuid}");
                            if (data.Item1 == System.Net.HttpStatusCode.OK)
                            {
                                List<StackInfo> stack = JsonConvert.DeserializeObject<List<StackInfo>>(data.Item2);

                                foreach (StackInfo e in stack)
                                {
                                    if (e.stack_owner == null) continue;
                                    if (stack_dict.ContainsKey(e.stack_owner))
                                    {
                                        if (stack_dict[e.stack_owner] + 4 <= e.stack_size)
                                        {
                                            for (int n = 4; n > 1; --n)
                                            {
                                                if (stack_dict[e.stack_owner] + 4 * n <= e.stack_size)
                                                {
                                                    pop_stack += n;
                                                    stack_dict[e.stack_owner] += 4 * n;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        stack_dict[e.stack_owner] = e.stack_size;
                                    }
                                }
                            }
                        }
                        catch
                        {
                            Invoke(new Action(() =>
                            {
                                if (!(this.Disposing || this.IsDisposed))
                                    this.Text = "Offline(Failed to getStack)";
                            }));
                        }
                        if (push_stack > 0)
                        {
                            try
                            {
                                var data = sv.SendRequest("addStack", "POST", null, $"x-id: {my_uuid}");
                                --push_stack;
                            }
                            catch
                            {
                                Invoke(new Action(() =>
                                {
                                    if (!(this.Disposing || this.IsDisposed))
                                        this.Text = "Offline(Failed to addStack)";
                                }));
                            }
                        }
                    }
                }
            }
            Invoke(new Action(() =>
            {
                if (!(this.Disposing || this.IsDisposed))
                    this.Text = "Offline(Disconnected)";
                my_game_Paint(null, null);
            }));
        }

        void UpdateMyField()
        {
            my_field.board.Clear();
            FieldBlock fb = new FieldBlock();
            for (int x = 0; x < board_w; ++x)
            {
                List<FieldBlock> fbs = new List<FieldBlock>();
                for (int y = 0; y < board_h; ++y)
                {
                    if (map[x, y + hidden_board_h] != null)
                    {
                        fb.Color = map[x, y + hidden_board_h].color;
                        fb.isEmpty = false;
                    }
                    else
                    {
                        fb.Color = "";
                        fb.isEmpty = true;
                    }
                    fbs.Add(new FieldBlock { Color = fb.Color, isEmpty=fb.isEmpty});
                }
                my_field.board.Add(fbs);
            }
            my_field.score = my_score;
        }
        void UpdateOtherField()
        {
            if (other_field != null)
            {
                int x = 0;
                foreach (List<FieldBlock> fbs in other_field.board)
                {
                    int y = 0;
                    foreach (FieldBlock fb in fbs)
                    {
                        if (fb.isEmpty)
                        {
                            other_map[x, y] = null;
                        }
                        else
                        {
                            other_map[x, y] = new BlockForForm { isEmpty = false, color = fb.Color };
                        }
                        ++y;
                    }
                    ++x;
                }
                other_score = other_field.score;
            }

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
            now_angle = 0;
            now_y = 4 - block_template[now_block].Item1[now_angle].height;
            FillTemplate(now_block, now_angle, now_x, now_y);
            RefreshGhost();
        }

        void RefreshGhost()
        {
            ghost.Clear();
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
                        map[x + dx, y + dy] = new BlockForForm { isEmpty = false, color = color, isMoving = moving };
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
        private void FormGame_KeyDown(object sender, KeyEventArgs e)
        {
            if (!connection) return;
            if (gameover)
            {
                if (e.KeyCode == Keys.R)
                {
                    gameover = false;
                    stop = false;
                    queue = new List<string>();
                    ghost = new List<Point>(); 
                    map = new BlockForForm[board_w, board_h + hidden_board_h];

                    for (int i = 0; i < 3; ++i)
                    {
                        FillQueue();
                    }
                    DropNewBlock();
                    my_game_Paint(null, null);
                }
            }
            if (stop) return;
            switch (e.KeyCode)
            {
                //case Keys.D1:
                //    var trd_stt = new ParameterizedThreadStart(InsertLines);
                //    trd_insertline = new Thread(trd_stt);
                //    trd_insertline.Start((object)4);
                //    break;
                case Keys.A:
                    if (now_block != null)
                    {
                        EraseTemplate(now_block, now_angle, now_x, now_y);
                        CheckTemplateResult res = CheckTemplate(now_block, now_angle, now_x - 1, now_y);
                        if (res == CheckTemplateResult.OK) now_x -= 1;
                        FillTemplate(now_block, now_angle, now_x, now_y, true);

                        RefreshGhost();
                    }
                    my_game_Paint(null, null);
                    break;
                case Keys.ShiftKey:
                    if (now_block != null)
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
                            now_angle = 0;
                            now_y = 4 - block_template[now_block].Item1[now_angle].height;
                            RefreshGhost();
                        }
                    }
                    my_game_Paint(null, null);
                    break;
                case Keys.S:
                    if (now_block != null)
                    {
                        drop_ticks = 0;
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
                    my_game_Paint(null, null);
                    break;
                case Keys.D:
                    if (now_block != null)
                    {
                        EraseTemplate(now_block, now_angle, now_x, now_y);
                        CheckTemplateResult res = CheckTemplate(now_block, now_angle, now_x + 1, now_y);
                        if (res == CheckTemplateResult.OK) now_x += 1;
                        FillTemplate(now_block, now_angle, now_x, now_y, true);

                        RefreshGhost();
                    }
                    my_game_Paint(null, null);
                    break;
                case Keys.Q:
                    {
                        EraseTemplate(now_block, now_angle, now_x, now_y);

                        CheckTemplateResult res = CheckTemplate(now_block, (now_angle + block_template[now_block].Item1.Count - 1) % block_template[now_block].Item1.Count, now_x, now_y);
                        if (res == CheckTemplateResult.OK) now_angle = (now_angle + block_template[now_block].Item1.Count - 1) % block_template[now_block].Item1.Count;
                        FillTemplate(now_block, now_angle, now_x, now_y, true);

                        RefreshGhost();
                    }
                    my_game_Paint(null, null);
                    break;
                case Keys.E:
                    if (now_block != null)
                    {
                        EraseTemplate(now_block, now_angle, now_x, now_y);

                        CheckTemplateResult res = CheckTemplate(now_block, (now_angle + 1) % block_template[now_block].Item1.Count, now_x, now_y);
                        if (res == CheckTemplateResult.OK) now_angle = (now_angle + 1) % block_template[now_block].Item1.Count;
                        FillTemplate(now_block, now_angle, now_x, now_y, true);

                        RefreshGhost();
                    }
                    my_game_Paint(null, null);
                    break;
                case Keys.Space:
                    if (now_block != null)
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
                    my_game_Paint(null, null);
                    break;
            }
        }

        void CheckGameOver()
        {
            for (int x = 0; x < board_w; ++x)
            {
                for (int y = 0; y < hidden_board_h; ++y)
                {
                    if (map[x, y] != null)
                    {
                        if (!(map[x, y].isMoving || map[x, y].isEmpty))
                        {
                            gameover = true;
                            my_game_Paint(null, null);
                            stop = true;
                            my_score = 0;
                            return;
                        }
                    }
                }
            }
        }
        void RemoveLines(object lines)
        {
            List<int> list = (List<int>)lines;
            stop = true;

            Thread.Sleep(500);
            int n = 0;
            foreach (int y in list)
            {
                bool gray = false;
                for (int x = 0; x < board_w; ++x)
                {
                    if (map[x, y + hidden_board_h] != null)
                    {
                        if (map[x, y + hidden_board_h].color == "Gray") gray = true;
                        map[x, y + hidden_board_h] = null;
                    }
                }
                if (!gray) ++n;
            }
            push_stack += n * n;
            Invoke(new Action(() => {
                RefreshGhost();
                if (!(this.Disposing || this.IsDisposed))
                    my_game_Paint(null, null);
                }));

            int time = 0;
            foreach (int Y in list)
            {
                my_score += score_n_line[time];
                ++time;
                for (int y = Y; y > 1; --y)
                {
                    for (int x = 0; x < board_w; ++x)
                    {
                        map[x, y + hidden_board_h] = map[x, y - 1 + hidden_board_h];
                        map[x, y - 1 + hidden_board_h] = null;
                    }
                }
            }
            Invoke(new Action(() => {
                RefreshGhost();
                if (!(this.Disposing || this.IsDisposed))
                    my_game_Paint(null, null);
            }));

            stop = false;
            return;
        }
        void InsertLines(object e)
        {
            int count = (int)e;
            if (count == 0) return;
            stop = true;

            for (int i = 0; i < count; ++i)
            {

                EraseTemplate(now_block, now_angle, now_x, now_y);
                for (int y = 0; y < board_h; ++y)
                {
                    for (int x = 0; x < board_w; ++x)
                    {
                        map[x, y + hidden_board_h - 1] = map[x, y + hidden_board_h];
                        map[x, y + hidden_board_h] = null;
                    }
                }
                int j = rand.Next(0, board_w);
                for (int x = 0; x < board_w; ++x)
                {
                    if (j != x)
                        map[x, board_h - 1 + hidden_board_h] = new BlockForForm {color="Gray", isEmpty=false, isMoving=false };
                }
                now_y -= 1;
                if (now_y < 0) now_y = 0;
            }

            FillTemplate(now_block, now_angle, now_x, now_y);
            Invoke(new Action(() => {
                RefreshGhost();
                if (!(this.Disposing || this.IsDisposed))
                    my_game_Paint(null, null);
            }));

            stop = false;
        }

        private void FormGame_Load(object sender, EventArgs e)
        {
            trd_connection = new Thread(new ThreadStart(ConnectionControl));
            trd_connection.Start();
        }
        private void my_game_Paint(object sender, PaintEventArgs e)
        {
            my_data_fresh = true;

            bg_mygame.Graphics.Clear(Color.FromArgb(127, 127, 127));

            #region div_MainGame
            for (int x = 0; x < board_w; ++x)
            {
                for (int y = 0; y < board_h; ++y)
                {
                    if (map[x, y + hidden_board_h] != null)
                    {
                        bg_mygame.Graphics.DrawImage(block_image[map[x, y + hidden_board_h].color], div_MainGame.X + x * 24, div_MainGame.Y + y * 24);
                    }
                    else
                    {
                        bg_mygame.Graphics.DrawImage(block_image["Black"], div_MainGame.X + x * 24, div_MainGame.Y + y * 24);
                    }
                }
            }
            foreach (Point p in ghost)
            {
                bg_mygame.Graphics.FillRectangle(ghost_brush, div_MainGame.X + p.x * 24, div_MainGame.Y + (p.y - hidden_board_h) * 24, 24, 24);
            }
            #endregion

            #region div_Hold
            sf_center.LineAlignment = StringAlignment.Near;
            bg_mygame.Graphics.DrawString("HOLD", fontSmall, Brushes.Black, div_Hold.X + 24 * 2, div_Hold.Y, sf_center);
            sf_center.LineAlignment = StringAlignment.Center;
            for (int x = 0; x < 4; ++x)
            {
                for (int y = 0; y < 4; ++y)
                {
                    bg_mygame.Graphics.DrawImage(block_image["Black"], div_Hold.X + x * 24, div_Hold.Y + y * 24 + 24);
                }
            }
            if (hold_block.Length > 0)
            {
                var temp = block_template[hold_block];
                var block = temp.Item1[0];
                for (int x = 0; x < 4; ++x)
                {
                    for (int y = 0; y < 4; ++y)
                    {
                        if (block.isBlocked[x, y])
                        {
                            bg_mygame.Graphics.DrawImage(block_image[temp.Item2], div_Hold.X + x * 24 + 48 - block.cenx * 24, div_Hold.Y + y * 24 + 48 - block.ceny * 24 + 24);
                        }
                    }
                }
            }
            #endregion

            #region div_Next
            sf_center.LineAlignment = StringAlignment.Near;
            bg_mygame.Graphics.DrawString("NEXT", fontSmall, Brushes.Black, div_Next.X + 24 * 2, div_Next.Y, sf_center);
            sf_center.LineAlignment = StringAlignment.Center;
            for (int time = 0; time < 3; ++time)
            {
                for (int x = 0; x < 4; ++x)
                {
                    for (int y = 0; y < 4; ++y)
                    {
                        bg_mygame.Graphics.DrawImage(block_image["Black"], div_Next.X + x * 24, div_Next.Y + y * 24 + time * 24 * 4.5f + 24);
                    }
                }
            }

            if (queue.Count > 2)
            {
                for (int time = 0; time < 3; ++time)
                {
                    var temp = block_template[queue[time]];
                    var block = temp.Item1[0];
                    for (int x = 0; x < 4; ++x)
                    {
                        for (int y = 0; y < 4; ++y)
                        {
                            if (block.isBlocked[x, y])
                            {
                                bg_mygame.Graphics.DrawImage(block_image[temp.Item2], div_Next.X + x * 24 + 48 - block.cenx * 24, div_Next.Y + y * 24 + 48 - block.ceny * 24 + time * 24 * 4.5f + 24);
                            }
                        }
                    }
                }
            }
            #endregion

            #region div_score
            sf_center.LineAlignment = StringAlignment.Near;
            sf_center.Alignment = StringAlignment.Near;
            bg_mygame.Graphics.DrawString($"Score: {my_score}", fontSmall, Brushes.Black, div_Score.X, div_Score.Y, sf_center);
            //bg_mygame.Graphics.DrawString($"{my_uuid}", fontSmall, Brushes.Black, div_Score.X, div_Score.Y + 30, sf_center);
            //bg_mygame.Graphics.DrawString($"{push_stack}", fontSmall, Brushes.Black, div_Score.X, div_Score.Y + 60, sf_center);
            //bg_mygame.Graphics.DrawString($"{pop_stack}", fontSmall, Brushes.Black, div_Score.X, div_Score.Y + 90, sf_center);
            sf_center.LineAlignment = StringAlignment.Center;
            sf_center.Alignment = StringAlignment.Center;
            #endregion

            if (gameover)
            {
                bg_mygame.Graphics.FillRectangle(gameover_brush, div_MainGame.X, div_MainGame.Y, 24 * 10, 24 * 20);
                bg_mygame.Graphics.DrawString($"Game Over", fontBig, Brushes.White, div_MainGame.X + 24 * 5, div_MainGame.Y + 24 * 10, sf_center);
                bg_mygame.Graphics.DrawString($"Press r to restart", fontSmall, Brushes.White, div_MainGame.X + 24 * 5, div_MainGame.Y + 24 * 12, sf_center);
            }

            if (!connection)
            {
                bg_mygame.Graphics.FillRectangle(gameover_brush, div_MainGame.X, div_MainGame.Y, 24 * 10, 24 * 20);
                bg_mygame.Graphics.DrawString($"Disconnected", fontBig, Brushes.White, div_MainGame.X + 24 * 5, div_MainGame.Y + 24 * 10, sf_center);
            }

            bg_mygame.Render();

        }
        private void otherGame_Paint(object sender, PaintEventArgs e)
        {
            bg_othgame.Graphics.Clear(Color.FromArgb(127, 127, 127));

            #region div_MainGame
            for (int x = 0; x < board_w; ++x)
            {
                for (int y = 0; y < board_h; ++y)
                {
                    if (other_map[x, y] != null && !other_map[x, y].isEmpty)
                    {
                        bg_othgame.Graphics.DrawImage(block_image[other_map[x, y].color], div_MainGame.X + x * 24, div_MainGame.Y + y * 24);
                    }
                    else
                    {
                        bg_othgame.Graphics.DrawImage(block_image["Black"], div_MainGame.X + x * 24, div_MainGame.Y + y * 24);
                    }
                }
            }
            #endregion
                

            #region div_score
            sf_center.LineAlignment = StringAlignment.Near;
            sf_center.Alignment = StringAlignment.Near;
            bg_othgame.Graphics.DrawString($"Score: {other_score}", fontSmall, Brushes.Black, div_Score.X, div_Score.Y, sf_center);
            if (other_field != null)
            {
                //bg_othgame.Graphics.DrawString($"{other_field.uuid}", fontSmall, Brushes.Black, div_Score.X, div_Score.Y + 30, sf_center);
                if (other_field.date != null) bg_othgame.Graphics.DrawString($"{other_field.date.Substring(10, 8)}", fontSmall, Brushes.Black, div_Score.X, div_Score.Y + 30, sf_center);
            }
            sf_center.LineAlignment = StringAlignment.Center;
            sf_center.Alignment = StringAlignment.Center;
            #endregion
            
            bg_othgame.Render();
        }

        private void FormGame_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (trd_removeline != null) trd_removeline.Join();
            if (trd_connection.IsAlive)
            {
                connection = false;
                main_timer.Enabled = false;
                e.Cancel = true;
                return;
            }
            if (trd_connection != null)
            {
                trd_connection.Join();
            }
        }

        private void main_timer_Tick(object sender, EventArgs e)
        {
            if (stop) return;
            
            if (now_block == null)
            {
                CheckGameOver();
                DropNewBlock();
                my_game_Paint(null, null);
            }

            ++drop_ticks;
            if (drop_ticks > 10)
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

                my_game_Paint(null, null);
            }

            
            List<int> lists = new List<int>();

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
                    lists.Add(y);
                }
            }

            if (lists.Count > 0)
            {
                var trd_stt = new ParameterizedThreadStart(RemoveLines);
                trd_removeline = new Thread(trd_stt);
                trd_removeline.Start((object)lists);
            }
            else
            {
                CheckGameOver();
            }

            if (pop_stack > 0)
            {
                var trd_stt = new ParameterizedThreadStart(InsertLines);
                trd_insertline = new Thread(trd_stt);
                int obj = pop_stack;
                trd_insertline.Start((object)obj);
                pop_stack = 0;
            }
        }
    }
}
