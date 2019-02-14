using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace _0115_オセロ
{
    public partial class Form1 : Form
    {
        private int[,] board;
        private int[,] ten;
        private const int Shiro = 0;  //盤
        private const int Kuro = 1;  // 白　定数
        private const int Nashi = -1;  //黒　定数
        private const int Kabe = -2;  //空
        private String[] IroName;  //色の名前

        private int turn = Kuro;
        private int oitaIshi = 4;
        private bool pass1 = false;
        

        //8つの方向
        //       →　ｘ　
        //       5 6 7
        //   ↓  4 * 0
        //    y  3 2 1
        // この方向を表すための配列
        private int[] DX;
        private int[] DY;

        //人かＡＩか　黒:playerS[Kuro] / 白：playerS[Shiro]　, 人:Hito/AI:AI1
        private int[] playerS = new int[2];
        private const int Hito = 0;
        private const int AI1 = 1;
        private bool thinking = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
        //**********************************************************
        //プログラム開始時のコード
        private void Form1_Load(object sender, EventArgs e)
        {
            reset();
        }
        //********************************************************
        //初期化
        private void reset()
        {
            board = new int[10, 10];//盤を表す配列
            for (int i = 0; i < 10; i++)
            {
                for (int x = 0; x < 10; x++)
                {
                    board[i, x] = Nashi;//空で初期化
                }
            }
            for (int i = 0; i < 10; i++)
            {
                board[i, 0] = Kabe;
                board[i, 9] = Kabe;
                board[0, i] = Kabe;
                board[9, i] = Kabe;
            }
            board[4, 4] = Shiro;
            board[5, 5] = Shiro;
            board[4, 5] = Kuro;
            board[5, 4] = Kuro;
            //8つの方向
            //       →　ｘ　
            //       5 6 7
            //   ↓  4 * 0
            //    y  3 2 1
            DX = new int[8] { 1, 1, 0, -1, -1, -1, 0, 1 };
            DY = new int[8] { 0, 1, 1, 1, 0, -1, -1, -1 };

            //プレイヤー選択ボタンの設定
            radioButton1.Checked = true;
            playerS[0] = Hito;//黒は人
            radioButton3.Checked = true;
            playerS[1] = Hito;//白も人

            //色の名前文字列を作っておく
            IroName = new string[2];
            IroName[Kuro] = "黒";
            IroName[Shiro] = "白";

            //ダミーのビットマップを作っておく
            Bitmap b = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Image = b;
            //盤を表示する
            drawBoard();
        }
        //**************************************************************
        //表示
        //board[] のデータをＧＵＩ画面に表示する

        private void drawBoard()
        {
            //新しいBitmapを作り,そこに描画する
            Bitmap b = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(b);
            Pen p = new Pen(Color.Black, 2);
            //線を引く
            for (int i = 1; i < 8; i++)
            {
                g.DrawLine(p, 0, i * 60, 480, i * 60);
                g.DrawLine(p, i * 60, 0, i * 60, 480);
            }
            //board配列の中身を描画
            for (int y = 1; y <= 8; y++)
            {
                for (int x = 0; x <= 8; x++)
                {
                    if (board[y, x] == Kuro)
                    {   //黒なら
                        g.FillEllipse(Brushes.White, (x - 1) * 60 + 4, (y - 1) * 60 + 4, 56, 56);
                        g.FillEllipse(Brushes.Black, (x - 1) * 60 + 2, (y - 1) * 60 + 2, 56, 56);
                 
                    }
                    else if (board[y, x] == Shiro)
                    {  //白なら
                        g.FillEllipse(Brushes.Black, (x - 1) * 60 + 4, (y - 1) * 60 + 4, 56, 56);
                        g.FillEllipse(Brushes.White, (x - 1) * 60 + 2, (y - 1) * 60 + 2, 56, 56);
                    }
                }
            }
            p.Dispose();
            g.Dispose();
            pictureBox1.Image.Dispose();
            pictureBox1.Image = b;
            this.Refresh();
            Thread.Sleep(100);
        }
        //**********************************************************************
        //反対　　白と黒反対のいろを返す
        private int hantai(int iro)
        {
            if (iro == Kuro)
            {
                return Shiro;
            }
            else
            {
                return Kuro;
            }
        }
        //***********************************************************************
        //ある方向にひっくり返せる石があるかどうか
        private int kaeseruIshiNoKazu(int iro, int x, int y, int houkou)
        {
            int ret = 0;
            int aite = hantai(iro);
            int dx = DX[houkou];
            int dy = DY[houkou];
            for (int i = 1; i < 8; i++)
            {
                if (board[y + i * dy, x + i * dx] == aite)//相手の石なら++
                {
                    ret++;
                }
                else if (board[y + i * dy, x + i * dx] == iro)//自分の石なら
                {
                    return ret;
                }
                else    //壁、空が出てき時
                {
                    return 0;
                }
            }
                return 0;
        }
        //***********************************************************************
        //ある場所に石を置くと、返せる石がいくつあるか
        //置けるかもしらべる
        private int kaeseruIshiNoKazuGoukei(int iro, int x, int y)
        {
            if (board[y, x] != Nashi)//この座標に石がおいてある
            {
                return 0;//おけない
            }
            int c = 0;  //返せる石の合計
            for (int houkou = 0; houkou < 8; houkou++)//各方向について調べる
            {
                c=c+kaeseruIshiNoKazu(iro, x, y, houkou);
            }
            return c;
        }
        //***********************************************************************
        //石を置く
        private void ishiWoOku(int iro, int x, int y)
        {
            if (kaeseruIshiNoKazuGoukei(iro, x, y) <= 0)//石が置けないエラー
            {
                label1.Text = "エラー:置けない所に置こうとした";
                throw new InvalidOperationException();//強制終了
            }

            board[y, x] = iro;//石置く
            oitaIshi++;//置いた医師の数を数える変数を++
            drawBoard();//とりあえず描画
            for (int houkou = 0; houkou < 8; houkou++)//すべての方向をひっくり返す
            {
                int dx = DX[houkou];
                int dy = DY[houkou];
                int c = kaeseruIshiNoKazu(iro, x, y, houkou);//その方向で返せる石の数を計算し
                for (int i = 1; i <= c; i++)//その数だけひっくり返す
                {
                    board[y+i*dy,x+i*dx]=iro;
                    drawBoard();  //返すたびに描画
                }
            }
        }

        //***************************************************************************
        //人の出番でマウスがクリックされたとき
        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            //人の出番かどうか確認するべき

            //クリックされた座標から、盤面のx,yを計算する
            int xi = e.X / 60 + 1;
            int yi = e.Y / 60 + 1;
            if (kaeseruIshiNoKazuGoukei(turn, xi, yi) <= 0)
            {
                label1.Text = "そこにはおけません";
                return;
            }
            label1.Text = "おいた";
            ishiWoOku(turn, xi, yi);//石を置く
            turn = hantai(turn);//反対のターン
        }
        //***********************************************************************
        //終了のメッセージ表示
        private void syuuryou(string msg)
        {
            //盤面の黒と白の数を数える
            int k = 0, s = 0;
            for (int i = 1; i <= 8; i++)
            {
                for (int j = 1; j <= 8; j++)
                {
                    if (board[j, i] == Kuro)
                    {
                        k++;
                    }
                    else if (board[j, i] == Shiro)
                    {
                        s++;
                    }
                }
            }
            //勝敗を描く
            if (k > s)
            {
                MessageBox.Show(msg + k.ToString() + ";" + s.ToString() + "で黒の勝ち", "終了");
            }
            else if (k < s)
            {
                MessageBox.Show(msg + k.ToString() + ";" + s.ToString() + "で白の勝ち", "終了");
            }
            else
            {
                MessageBox.Show(msg + k.ToString() + ";" + s.ToString() + "引き分け", "終了");
            }
            label1.Text = msg;
        }
        //*************************************************************************************************************************
        //どこかに石を置けるか
        private Boolean dokokaniIhiwoOkeruka(int iro)
        {
            for (int i = 1; i <= 8; i++)
            {
                for (int j = 1; j <= 8; j++)
                {
                    if (kaeseruIshiNoKazuGoukei(iro, i, j) > 0)//0個より石が返せる⇒石が置ける
                    {
                        kaeseruIshiNoKazuGoukei(iro, i, j);
                    }
                }
            }
            return false;
        }
        //*****************************************************************************************************************************
        //タイマーで起動メソッド
        private void timer1_Tick(object sender, EventArgs e)
        {
            turnJikkou();
        }
        //******************************************************************************************************************************
        //黒　人　ボタン
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            playerS[Kuro] = Hito;
        }
        //黒　AI　ボタン
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            playerS[Kuro] = AI1;
            if (thinking == false)//AI実行中ではないときに切り替わった
            {
                timer1.Enabled = true;
            }
        }
        //白　人　ボタン
        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            playerS[Shiro] = Hito;
        }
        //白　AI　ボタン
        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            playerS[Shiro] = AI1;
            if (thinking == false)//AIが実行中ではないときに切り替わった
            {
                timer1.Enabled = true;
            }
        }
        //****************************************************************************************************************
        //ターン実行
        private void turnJikkou()
        {
            //終了判定
            if (oitaIshi >= 64)//盤面の石の数が64ならおしまい
            {
                timer1.Enabled = false;
                syuuryou("最後まで行きました");
                return;//終了する
            }
            if (dokokaniIhiwoOkeruka(turn))//パスをするかの判定
            {
                if (pass1)
                {
                    timer1.Enabled = false;
                    syuuryou("両者ともに石がおけません");
                    return;//終了
                }
                this.label1.Text = IroName[turn] + "おけないのでパス1";
                pass1 = true;//直前の相手はパスしていない。ここで私がパス
                turn = hantai(turn);//turnを交代して相手の番へ
                return;
            }
            //

            pass1 = false;//おけることが判明　pass1をfalseに設定
            if (playerS[turn] == Hito)//このターンが人ならループから抜ける
            {
                this.label1.Text = IroName[turn] + "さんどうぞ";
                return;
            }
            //このターンはAIだ
            thinking = true;//処理中というフラグが勝手にタイマーを再開させないように
            timer1.Enabled = false;//timerを一時停止
            this.label1.Text = IroName[turn] + "のターン";
            alg0();//AIを呼ぶ
            turn = hantai(turn);//ターン交代
            timer1.Enabled = true;
            thinking = false;
        }
        //**********************************************************************************************************************
        //盤面の左上から探して。石を置ける場所があってら置く
        private void alg0()
        {
            int iro = turn;
            for (int y = 1; y <= 8; y++)
            {
                for (int x = 1; x <= 8; x++)
                {
                    if (kaeseruIshiNoKazuGoukei(iro, x, y) > 0)
                    {
                        ishiWoOku(iro, x, y);
                        return;
                    }
                }
            }
        }
        private void alg1()
        {
            int maxx = 0, maxy = 0;
            int maxatai = -21;
            ten = new int[10, 10];//盤を表す配列
            for (int i = 0; i < 10; i++)
            {
                ten[i, 0] = Kabe;
                ten[i, 9] = Kabe;
                ten[0, i] = Kabe;
                ten[9, i] = Kabe;
            }
            ten[1, 1] = 120;
            ten[1, 8] = 120;
            ten[8, 1] = 120;
            ten[8, 8] = 120;

            ten[1,2]=-20;
            ten[1,7]=-20;
            ten[2,1]=-20;
            ten[2,8]=-20;
            ten[7,1]=-20;
            ten[7,8]=-20;
            ten[8,2]=-20;
            ten[8,7]=-20;

            ten[1,4]=5;
            ten[1,5]=5;
            ten[4,1]=5;
            ten[4,8]=5;
            ten[5,1]=5;
            ten[5,8]=5;
            ten[8,4]=5;
            ten[8,5]=5;

            ten[1,3]=20;
            ten[1,6]=20;
            ten[3,1]=20;
            ten[3,8]=20;
            ten[6,1]=20;
            ten[6,8]=20;
            ten[8,3]=20;
            ten[8,6]=20;

            ten[2,3]=-5;
            ten[2,4]=-5;
            ten[2,5]=-5;
            ten[2,6]=-5;
            ten[3,2]=-5;
            ten[4,2]=-5;
            ten[5,2]=-5;
            ten[6,2]=-5;
            ten[3,7]=-5;
            ten[4,7]=-5;
            ten[5,7]=-5;
            ten[6,7]=-5;
            ten[7,3]=-5;
            ten[7,4]=-5;
            ten[7,5]=-5;
            ten[7,6]=-5;

            ten[3,3]=15;
            ten[3,6]=15;
            ten[6,3]=15;
            ten[6,6]=15;

            ten[3,4]=5;
            ten[3,5]=5;
            ten[4,3]=5;
            ten[4,4]=5;
            ten[4,5]=5;
            ten[4,6]=5;
            ten[5,3]=5;
            ten[5,4]=5;
            ten[5,5]=5;
            ten[5,6]=5;
            ten[6,4]=5;
            ten[6,5]=5;

            int iro = turn;
            for (int y = 1; y <= 8; y++)
            {
                for (int x = 1; x <= 8; x++)
                {
                    if (kaeseruIshiNoKazuGoukei(iro, x, y) > 0)
                    {
                        if (maxatai < ten[x, y])
                        {
                            maxatai = ten[x, y];
                            maxx = x;
                            maxy = y;
                        }
                    }
                }
            }
            if (maxx > 0)
            {
                ishiWoOku(iro, maxx, maxy);
                return;
            }
        }
    }
}
