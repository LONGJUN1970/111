using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;
using System.IO;
using System;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;
using System.Linq;
using ViveSR.anipal.Eye;
//  やるべきこと,検討すること
//  Lの密度 /
// 説明の資料 /
// fove記録の意味理解および解析
// 必要なら手動でのブロック分け
// 位置座標を使わないことも検討
//main




public class mainscript : MonoBehaviour
{
    //被験者決定項目
    string subjectname = "HaT";
    public static int experimentcount = 6;//実験カウント。何回目から

    public GameObject textT;
    public GameObject textTPrefab;
    public GameObject textL;
    public GameObject textLPrefab;
    public GameObject fix;
    public GameObject fixPrefab;
    public GameObject fix2Prefab;
    public GameObject fix3Prefab;
    public GameObject arrow;
    public GameObject arrowPrefab;
    GameObject tmpobj;

    public bool DestroyT = false;
    public bool DestroyL = false;
    public bool Destroyfix = false;
    public bool Destroyfix2 = false;
    public bool DestroyArrow = false;
    bool trial = false;
    bool Tright = false;
    public bool start = true;
    bool pushR = false;
    bool pushL = false;
    public bool recog = true;//30が終わったかどうかを確認する？
    public bool recogd = false;
    bool lagflag = false;
    bool starttrial = false;
    bool stopflag = false;
    public bool transferflag = true;

    string filePath1;
    string filePath2;
    string filePath3;
    string filePath4;
    string filePathtest;


    public static int gridx = 24;                 //grid(刺激を配置する格子)の横の数　回転角度＝(360 / gridx)°　小さすぎると隣り合う文字がぶつかる
    public static int gridy = 6;                  //gridの縦の数　
    public static int gridnum = gridx * gridy;
    public static int grideverydis = gridnum / 6;
    public static float radius = 5.72f;                   //gridが作る円の半径（長さ単位[m])
    public static float lenVer = 0.5f;              //grid1つのの縦の長さ
    public static int trialnum = 25;                //1ブロック試行回数
    public static int blocknum = 30;                //ブロック数 30
    public static int[] array;                     //
    public static int sametrialnum = 13;            //繰り返しの試行回数
    public static int objLnum = 35;                        //ディストラクタLの数(T含まず)
    public static float timetrial;                         //試行ごとの時間
    public static int trialcount = 0;                      //試行回数
    public static int breaknum = 6;                 //何ブロック毎に休憩するか　blocknumの約数にする
    public static int blockcount = 1 + breaknum * (experimentcount - 1);                      //現在のブロック
    public static int[][] arraysame = new int[sametrialnum][];
    public static int[] arrayT;
    public static float[][] zitter;
    int separatenum = 12;
    int transnum;

    public static int[] order = new int[trialnum];                //ブロックごとに配列の順番を決める
    float timelag = 0f;
    float lag;
    float rnd1;
    int Tpos;
    string oldnew;
    int tmp;
    int width;

    public Material skyboxb;
    public Material skyboxg;

    [SerializeField] private bool calibrationcheck;


    private void Awake()
    {


    }
    // Use this for initialization
    void Start()
    {
        //ファイル位置
        filePath1 = "../result/" + subjectname + "/data.txt";
        filePath2 = "../result/" + subjectname + "/Vivedata.txt";
        filePath3 = "../result/" + subjectname + "/reyeVivedata.txt";
        filePath4 = "../result/" + subjectname + "/leyeVivedata.txt";
        filePathtest = "../result/" + subjectname + "/test.txt";


        if (calibrationcheck == true)
        {
            SRanipal_Eye_API.LaunchEyeCalibration(IntPtr.Zero);
        }

        for (int i = 0; i < trialnum; i++) order[i] = i;//1ブロックあたりの試行回数個の配列を用意
        arrayShuffle(trialnum, order);
        //////////////////////////////////////////////////////////////////////////////////////////////////
        for (int i = 0; i < sametrialnum; i++) arraysame[i] = new int[gridnum];//繰り返しの試行回数個の配列を用意
        for (int i = 0; i < trialnum; i++) order[i] = i;
        arrayShuffle(trialnum, order);
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //文字の位置が重複しないように配列を用いる
        //同じ配列を用いるために最初に定義する
        // 配列の初期化　
        //arraysame[繰り返し配置個][マス目の数]
        for (int k = 0; k < sametrialnum; k++)
        {
            for (int i = 0; i < gridx; i++)
            {
                for (int j = 0; j < gridy; j++)
                {
                    arraysame[k][i * gridy + j] = i * gridy + j;
                }
            }
            arrayShuffle(gridnum, arraysame[k]);
        }

        for (int i = 0; i < gridnum; i++) // 意識学習する最初の配列のTの位置を後ろに置く
        {
            if (arraysame[0][i] > ((gridx / 2) - 1) * gridy && arraysame[0][i] < ((gridx / 2) + 1) * gridy)
            {
                tmp = arraysame[0][0];
                arraysame[0][0] = arraysame[0][i];
                arraysame[0][i] = tmp;
                break;
            }

        }
        for (int i = 1; i < sametrialnum; i++) //old試行のTの位置を均等にしておく sametrialnum等の値によっては動作しない//////////////////////////////////////////////////////////////////////
        {
            for (int j = 1; j < gridnum; j++)
            {
                if (arraysame[i][j] > (gridx / (sametrialnum - 1)) * gridy * (i - 1) && arraysame[i][j] < (gridx / (sametrialnum - 1)) * gridy * i)
                {
                    tmp = arraysame[i][0];
                    arraysame[i][0] = arraysame[i][j];
                    arraysame[i][j] = tmp;
                    break;
                }
            }
        }
        // 刺激配置の均質化 どうかんがえてもT配置の前にやったほうが簡単　後で余裕があれば直す
        for (int i = 0; i < sametrialnum; i++)
        {
            int[] tmparray = new int[objLnum + 1];
            int[] flagarray = new int[gridnum + 2];
            int y = 0;
            int num = (objLnum + 1) / separatenum;
            for (int j = 0; j < separatenum; j++)
            {
                int x = 0;
                for (int k = 1; k < gridnum; k++)
                {
                    if (arraysame[i][k] > (gridnum / separatenum) * j && arraysame[i][k] <= (gridnum / separatenum) * (j + 1))
                    {
                        if (flagarray[arraysame[i][k]] == 0)
                        {
                            tmparray[j * num + x] = arraysame[i][k];
                            flagarray[arraysame[i][k]] = 1;
                            if (k != gridnum - 1) flagarray[arraysame[i][k] - 1] = 1;
                            flagarray[arraysame[i][k] + 1] = 1;
                            x++;
                        }
                    }
                    if (x == num) break;
                }
            }
            y = arraysame[i][0] / gridx;
            for (int a = 0; a < objLnum + 1; a++) arraysame[i][a + 1] = tmparray[a];
            tmp = arraysame[i][y * gridy + 1];
            arraysame[i][y * gridy + 1] = arraysame[i][objLnum + 1];
            arraysame[i][objLnum + 1] = tmp;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////
        ///新規配置？？
        arrayT = new int[12];
        //arrayTランダム
        for (int i = 0; i < 12; i++)
        {
            arrayT[i] = Random.Range(0, 12) + (i * 12);//12の倍数の間から一つずつ
        }
        //0～23が正面(1～24?)　24～47がどちらか120° 48～71が240°

        //arraysame[0] = new int[36] { 2, 10, 4, 20, 16, 13, 32, 27, 25, 37, 46, 43, 59, 49, 53, 67, 71, 62, 78, 75, 83, 85, 88, 92, 106, 98, 101, 119, 116, 111, 131, 128, 124, 133, 137, 140 };
        //arraysame[1] = new int[36] { 11, 7, 1, 13, 16, 18, 35, 25, 27, 44, 40, 42, 51, 49, 55, 60, 70, 68, 82, 79, 75, 86, 94, 90, 99, 104, 107, 114, 117, 109, 124, 121, 127, 136, 132, 142 };
        //arraysame[2] = new int[36] { 23, 10, 3, 12, 6, 18, 32, 29, 34, 38, 47, 36, 56, 51, 49, 60, 69, 71, 75, 79, 81, 87, 90, 95, 106, 103, 98, 113, 115, 117, 131, 127, 124, 139, 141, 135 };
        //arraysame[3] = new int[36] { 19, 3, 6, 13, 21, 0, 28, 32, 25, 46, 43, 37, 58, 49, 54, 67, 70, 63, 73, 83, 77, 93, 85, 88, 105, 96, 102, 119, 108, 113, 125, 122, 129, 137, 132, 135 };
        //arraysame[4] = new int[36] { 34, 4, 8, 16, 18, 13, 1, 25, 28, 36, 47, 42, 56, 52, 50, 61, 63, 70, 82, 79, 73, 93, 95, 87, 97, 99, 104, 119, 115, 109, 123, 126, 130, 135, 133, 137 };
        //arraysame[5] = new int[36] { 32, 10, 3, 21, 14, 23, 8, 27, 25, 41, 38, 46, 50, 57, 52, 63, 60, 69, 83, 74, 77, 88, 92, 95, 105, 98, 103, 113, 110, 117, 129, 127, 120, 139, 137, 132 };
        //arraysame[6] = new int[36] { 27, 11, 2, 18, 23, 21, 0, 30, 35, 47, 44, 40, 58, 49, 54, 60, 65, 68, 73, 75, 81, 89, 86, 91, 101, 98, 106, 108, 119, 114, 127, 123, 125, 134, 137, 139 };
        //arraysame[7] = new int[36] { 40, 10, 2, 20, 15, 18, 8, 30, 28, 34, 43, 38, 56, 53, 59, 61, 69, 67, 76, 73, 83, 87, 91, 93, 102, 100, 106, 119, 115, 111, 123, 128, 125, 136, 139, 143 };
        //arraysame[8] = new int[36] { 52, 9, 2, 18, 16, 12, 27, 30, 25, 40, 38, 46, 6, 55, 58, 61, 63, 68, 76, 80, 74, 84, 88, 90, 96, 106, 104, 114, 118, 109, 129, 122, 124, 137, 139, 141 };
        //arraysame[9] = new int[36] { 57, 0, 5, 20, 22, 12, 26, 34, 25, 38, 45, 41, 9, 49, 52, 60, 70, 67, 83, 73, 75, 87, 90, 94, 104, 101, 98, 119, 110, 112, 129, 122, 131, 135, 138, 141 };
        //arraysame[10] = new int[36] { 71, 1, 8, 14, 21, 12, 32, 29, 25, 46, 44, 36, 5, 55, 50, 58, 62, 64, 75, 78, 82, 90, 94, 85, 98, 105, 96, 119, 115, 117, 129, 127, 122, 141, 132, 143 };
        //arraysame[11] = new int[36] { 69, 2, 10, 21, 14, 23, 35, 32, 28, 37, 39, 44, 4, 58, 49, 54, 60, 5, 73, 77, 75, 90, 95, 84, 103, 106, 99, 108, 112, 110, 131, 121, 129, 143, 138, 134 };
        //arraysame[12] = new int[36] { 66, 9, 5, 13, 15, 19, 25, 0, 31, 44, 39, 42, 59, 49, 53, 62, 69, 33, 82, 80, 77, 85, 94, 90, 97, 107, 99, 117, 114, 112, 123, 120, 125, 142, 133, 139 };

   
        //繰り返し配置0～11　３方向に４つずつ　12が意識的記憶配置

        arraysame[0] = new int[36] { 2, 7, 10, 15, 20, 17, 35, 32, 25, 37, 46, 41, 55, 53, 49, 71, 61, 65, 77, 74, 82, 84, 95, 91, 107, 102, 97, 114, 118, 116, 131, 128, 125, 133, 136, 143 };
        arraysame[1] = new int[36] { 13, 7, 11, 23, 5, 21, 27, 25, 33, 45, 38, 36, 58, 55, 49, 62, 64, 67, 79, 77, 81, 93, 86, 89, 98, 101, 103, 112, 115, 117, 131, 126, 123, 134, 143, 138 };
        arraysame[2] = new int[36] { 11, 6, 4, 18, 16, 13, 29, 31, 25, 40, 36, 38, 51, 55, 53, 66, 64, 70, 74, 77, 80, 87, 84, 94, 105, 102, 96, 114, 117, 109, 126, 124, 128, 142, 134, 138 };
        arraysame[3] = new int[36] { 23, 3, 9, 6, 20, 16, 30, 28, 25, 38, 45, 47, 52, 54, 58, 62, 71, 67, 82, 73, 78, 92, 94, 88, 100, 102, 96, 119, 109, 112, 131, 127, 124, 138, 141, 143 };
        arraysame[4] = new int[36] { 38, 10, 6, 18, 15, 1, 20, 32, 24, 33, 45, 47, 56, 51, 54, 71, 61, 68, 73, 82, 77, 87, 91, 94, 105, 98, 96, 114, 111, 109, 131, 121, 124, 140, 137, 143 };    
        arraysame[5] = new int[36] { 35, 3, 6, 20, 15, 22, 0, 26, 28, 45, 38, 47, 50, 58, 55, 61, 63, 68, 75, 79, 83, 92, 94, 89, 107, 96, 100, 116, 111, 114, 130, 122, 125, 134, 141, 143 };
        arraysame[6] = new int[36] { 25, 2, 9, 12, 16, 22, 27, 4, 32, 46, 43, 39, 51, 54, 57, 70, 62, 65, 81, 78, 75, 85, 95, 92, 100, 97, 107, 116, 119, 110, 124, 126, 130, 135, 133, 139 };
        arraysame[7] = new int[36] { 47, 6, 11, 19, 16, 14, 30, 27, 32, 38, 9, 40, 54, 52, 49, 60, 69, 67, 74, 83, 79, 93, 85, 91, 107, 97, 104, 116, 118, 109, 123, 128, 131, 142, 138, 134 };
        arraysame[8] = new int[36] { 49, 11, 2, 17, 20, 23, 35, 32, 27, 47, 42, 40, 58, 8, 52, 71, 61, 64, 81, 73, 75, 94, 92, 88, 104, 107, 102, 114, 117, 110, 121, 130, 128, 141, 139, 143 };
        arraysame[9] = new int[36] { 58, 3, 0, 23, 19, 21, 35, 30, 28, 47, 42, 45, 7, 49, 52, 71, 67, 69, 81, 79, 77, 95, 84, 88, 103, 106, 99, 117, 114, 108, 120, 124, 128, 141, 134, 143 };
        arraysame[10] = new int[36] { 61, 0, 4, 16, 18, 13, 29, 32, 35, 43, 40, 37, 58, 48, 52, 11, 64, 69, 81, 74, 83, 85, 89, 92, 98, 106, 103, 118, 116, 113, 126, 124, 120, 141, 138, 132 };
        arraysame[11] = new int[36] { 69, 11, 2, 17, 20, 23, 30, 34, 26, 43, 41, 39, 48, 52, 56, 67, 6, 62, 73, 75, 78, 93, 86, 89, 107, 104, 96, 113, 116, 118, 123, 120, 128, 137, 140, 134 }; 
        arraysame[12] = new int[36] { 56, 3, 16, 23, 12, 20, 27, 31, 25, 42, 47, 38, 51, 55, 7, 61, 58, 67, 73, 77, 82, 87, 92, 94, 101, 98, 107, 116, 111, 114, 127, 124, 121, 138, 142, 134 };//左意識
        //arraysame[12] = new int[36] { 40, 3, 16, 23, 12, 20, 27, 31, 25, 42, 47, 62, 51, 55, 7, 61, 58, 67, 73, 77, 82, 87, 92, 94, 101, 98, 107, 116, 111, 114, 127, 124, 121, 138, 142, 134 };//右意識

        //arraysame[0] = new int[36] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36 };
        //arraysame[1] = new int[36] { 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72 };
        //arraysame[2] = new int[36] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36 };
        //arraysame[3] = new int[36] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36 };
        //arraysame[4] = new int[36] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36 };
        //arraysame[5] = new int[36] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36 };
        //arraysame[6] = new int[36] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36 };
        //arraysame[7] = new int[36] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36 };
        //arraysame[8] = new int[36] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36 };
        //arraysame[9] = new int[36] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36 };
        //arraysame[10] = new int[36] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36 };
        //arraysame[11] = new int[36] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36 };
        //arraysame[12] = new int[36] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36 };

        //新規配置のTの位置
        arrayT = new int[] { 7, 20, 16, 5, 41, 32, 28, 43, 64, 66, 55, 52 };
        //arrayT = new int[] { 3, 23, 18, 5, 37, 30, 25, 48, 64, 66, 48, 52 };
        //arrayT = new int []{5, 21, 33, 36, 51, 67, 82, 86, 98, 109, 124, 133};
        //arrayT = new int[] { 1,1,1,1,1,1,1,1,1,1,1,1};


        zitter = new float[trialnum][];
        for (int i = 0; i < trialnum; i++)
        {
            zitter[i] = new float[objLnum + 1];
            for (int j = 0; j < objLnum + 1; j++) zitter[i][j] = Random.Range(-5f, 5f);
        }
        zitter[0] = new float[36] { 0.7058911f, 4.699615f, 2.724941f, -1.036772f, 1.07246f, 2.952871f, -4.652634f, 4.224998f, 4.515122f, 4.573359f, 1.55325f, -0.04775f, 0.8706541f, -4.229002f, 1.599579f, -1.767147f, 3.516372f, 2.451476f, 3.042427f, 1.631346f, -1.131331f, -1.77049f, 3.649992f, -2.759383f, -3.593974f, -3.156021f, -1.026309f, 3.548923f, 0.453126f, 3.466476f, 4.233248f, 3.964321f, 1.728623f, 3.353568f, 2.137495f, 0.4396148f };
        zitter[1] = new float[36] { -1.521614f, 2.278538f, -1.45083f, 1.229332f, -2.813058f, -1.07755f, -2.592704f, 3.163687f, -1.553273f, 3.796452f, -2.45168f, -2.554173f, 3.630747f, 1.357783f, 0.1628308f, -1.186603f, -1.500836f, -0.5372782f, 0.06884527f, -3.551023f, -1.69728f, 1.174247f, 4.695613f, 4.835019f, -3.4019f, -2.993937f, -1.665063f, -1.623037f, 0.02545404f, 4.594749f, 2.888001f, -2.325137f, 1.9472f, -2.555262f, 0.569418f, -3.455935f };
        zitter[2] = new float[36] { 0.7525783f, 3.085151f, 1.236677f, 1.622833f, -1.498545f, -3.602566f, -1.718822f, 0.7630615f, -3.659134f, -0.6101565f, 2.036104f, -2.687787f, 1.27997f, -3.131864f, 3.469915f, 4.68535f, -4.304092f, -1.92895f, 3.455603f, 1.663408f, -1.192274f, 0.6264992f, 4.186332f, -1.175926f, -1.500419f, 2.963079f, 4.964134f, 4.144501f, -4.231924f, 2.77186f, -0.5402703f, -2.146575f, 3.421479f, -3.772368f, 3.72046f, 3.616932f };
        zitter[3] = new float[36] { 1.729589f, -3.66366f, -3.788555f, -1.418197f, -0.5785184f, 2.933968f, -4.976513f, -3.9739f, 2.825121f, -3.632459f, 2.467622f, 2.825589f, -0.7900481f, -2.562454f, -4.657654f, -2.071939f, 3.317195f, 3.166499f, 1.567485f, -4.350471f, 4.665121f, -4.086983f, -1.849349f, -3.12114f, 0.4028683f, -4.620104f, 1.479796f, 0.640461f, 1.279395f, -3.431978f, -4.311634f, -1.413261f, 1.898763f, 4.844241f, -4.512568f, -0.03459978f };
        zitter[4] = new float[36] { 2.429533f, 3.372948f, -3.847617f, 2.37625f, -4.978023f, -1.979525f, -2.268481f, -0.4321246f, 1.824837f, -3.832411f, -4.636134f, -1.042073f, -1.126852f, 0.1393957f, -0.496984f, -4.702092f, 4.823041f, 2.051753f, -2.102639f, 3.416451f, -2.84741f, -1.607034f, 3.34973f, -0.9083323f, 0.305057f, 3.459702f, -1.765955f, 1.6784f, -2.264383f, -4.176201f, -4.032069f, 1.771839f, -0.1029711f, 0.6340356f, 0.9968681f, 3.084021f };
        zitter[5] = new float[36] { -3.422624f, -2.826133f, 2.111363f, -1.955193f, 0.2485628f, 2.6846f, -1.00288f, 2.061971f, -0.7758551f, 0.3491373f, 4.624692f, -4.14819f, -4.943042f, 1.340274f, 1.790945f, -4.33392f, 1.507079f, 3.428004f, 2.323893f, 4.679071f, -1.162931f, -4.601089f, 0.1484318f, 2.443318f, 2.498814f, -2.496303f, -0.5798702f, -4.048219f, -0.8957548f, -1.276319f, 4.769033f, -3.891735f, -0.7017465f, -2.552321f, -3.541647f, 1.983607f };
        zitter[6] = new float[36] { 3.893774f, 4.554633f, 0.8970447f, -2.750841f, -0.4580717f, -1.947022f, 0.9044652f, 1.860838f, 3.92103f, -1.800346f, -4.401435f, -3.432817f, -4.200525f, 2.246946f, 0.8704453f, -1.638794f, 3.454703f, 2.100807f, -1.595819f, -0.589406f, -1.870226f, -2.987184f, 0.3102465f, -1.129946f, -0.7500668f, -2.820057f, -4.767413f, -2.953894f, -3.582609f, 4.652488f, -0.8175044f, -3.449426f, 1.5098f, -1.025273f, 1.485751f, -3.602705f };
        zitter[7] = new float[36] { -0.3099136f, -3.374697f, 3.140035f, 2.974f, -3.073234f, -0.2923636f, 3.439429f, 0.8186169f, -2.300473f, 1.651534f, 4.960934f, 3.170919f, 1.307554f, -3.307296f, -0.09975243f, 4.982272f, 2.890515f, -1.395667f, -2.428494f, -3.066882f, -2.554797f, 3.018192f, 2.01325f, 1.642532f, 3.685312f, 1.43646f, 2.374632f, 0.543056f, -2.267017f, -1.466555f, -3.928364f, -0.00848341f, 2.692271f, -4.055542f, 4.904202f, 2.356722f };
        zitter[8] = new float[36] { 4.557996f, 1.646987f, 2.166069f, -1.871325f, 0.2754645f, 3.996115f, -3.73122f, 0.7936783f, 2.376564f, -0.4119678f, -4.047824f, -4.116831f, 0.1268296f, 2.604082f, 2.017338f, -0.4497142f, -2.629024f, -4.830414f, 3.586206f, -3.005157f, 0.7133121f, -1.888867f, -2.94805f, -1.08522f, -4.493816f, -4.577837f, -0.6632514f, 2.068112f, 2.194011f, -2.616265f, 2.573997f, 1.236604f, 3.477512f, 1.767869f, -0.7127709f, -4.992414f };
        zitter[9] = new float[36] { 2.702394f, 0.6557317f, -3.162793f, 1.643782f, -4.309532f, 1.235351f, 0.4859786f, 3.634986f, -3.334327f, 3.77023f, 1.275192f, 0.7355981f, -2.920551f, 1.557341f, -3.044925f, -4.084433f, -0.05377579f, -0.8433037f, 3.060297f, 0.6510682f, -4.217821f, 3.398162f, 0.7967801f, -1.342378f, -1.673229f, 1.657253f, 4.361526f, -4.027229f, 2.869589f, 4.414273f, -4.731995f, -4.725537f, -4.836301f, 1.984385f, 0.03594351f, -2.063628f };
        zitter[10] = new float[36] { -1.752513f, 4.053068f, -3.840117f, 0.3832154f, -4.165722f, 4.961677f, 0.01392317f, 2.256374f, 0.1705236f, 3.992076f, -4.06415f, -3.934698f, 2.703416f, 1.870285f, -3.43103f, 4.291874f, -2.873733f, -0.6336756f, 4.280736f, 3.620958f, 3.172132f, 4.791121f, -3.098015f, -2.304211f, -3.30661f, 3.187222f, 0.9517531f, 1.789604f, -2.109018f, 2.881557f, 4.007866f, -1.850867f, 1.772371f, -4.104992f, -1.880407f, 2.171727f };
        zitter[11] = new float[36] { 4.487486f, -3.112659f, 2.330097f, -0.5357332f, -4.606728f, 1.990398f, -1.953412f, 4.477571f, 2.182083f, 1.64516f, 0.7291203f, 1.032269f, 3.520277f, -0.8206434f, -3.891525f, 3.920721f, 3.148434f, -2.763148f, -1.944995f, 4.764774f, 4.803525f, 3.940037f, 4.104003f, -3.10031f, 4.856091f, 2.965717f, 3.856848f, -1.35531f, -2.901413f, -4.682175f, -0.8807569f, -2.204614f, 4.398312f, 3.654405f, -0.5897279f, -0.2410502f };
        zitter[12] = new float[36] { 0.5630093f, 1.425485f, -2.2448f, -1.888079f, -3.634075f, -4.270067f, 3.085346f, 0.6295843f, 0.336215f, 0.2720518f, 4.485765f, -0.08517075f, 0.09749603f, -4.500179f, -4.203121f, -2.894029f, 2.635121f, 3.553579f, -2.933404f, -1.454902f, -4.18726f, 1.873855f, 2.310033f, 0.731092f, 3.716924f, 3.920895f, 1.678734f, -1.521323f, -3.625993f, -3.411275f, 2.764342f, -2.651605f, 4.836096f, -2.466774f, 0.7727532f, -0.5591865f };


        //zitter[0] = new float[36] { 0.7058911f, 4.699615f, 2.724941f, -1.036772f, 1.07246f, 2.952871f, -4.652634f, 4.224998f, 4.515122f, 4.573359f, 1.55325f, -0.04775f, 0.8706541f, -4.229002f, 1.599579f, -1.767147f, 3.516372f, 2.451476f, 3.042427f, 1.631346f, -1.131331f, -1.77049f, 3.649992f, -2.759383f, -3.593974f, -3.156021f, -1.026309f, 3.548923f, 0.453126f, 3.466476f, 4.233248f, 3.964321f, 1.728623f, 3.353568f, 2.137495f, 0.4396148f };
        //zitter[1] = new float[36] { 0.7058911f, 4.699615f, 2.724941f, -1.036772f, 1.07246f, 2.952871f, -4.652634f, 4.224998f, 4.515122f, 4.573359f, 1.55325f, -0.04775f, 0.8706541f, -4.229002f, 1.599579f, -1.767147f, 3.516372f, 2.451476f, 3.042427f, 1.631346f, -1.131331f, -1.77049f, 3.649992f, -2.759383f, -3.593974f, -3.156021f, -1.026309f, 3.548923f, 0.453126f, 3.466476f, 4.233248f, 3.964321f, 1.728623f, 3.353568f, 2.137495f, 0.4396148f };
        //zitter[2] = new float[36] { 0.7058911f, 4.699615f, 2.724941f, -1.036772f, 1.07246f, 2.952871f, -4.652634f, 4.224998f, 4.515122f, 4.573359f, 1.55325f, -0.04775f, 0.8706541f, -4.229002f, 1.599579f, -1.767147f, 3.516372f, 2.451476f, 3.042427f, 1.631346f, -1.131331f, -1.77049f, 3.649992f, -2.759383f, -3.593974f, -3.156021f, -1.026309f, 3.548923f, 0.453126f, 3.466476f, 4.233248f, 3.964321f, 1.728623f, 3.353568f, 2.137495f, 0.4396148f };
        //zitter[3] = new float[36] { 0.7058911f, 4.699615f, 2.724941f, -1.036772f, 1.07246f, 2.952871f, -4.652634f, 4.224998f, 4.515122f, 4.573359f, 1.55325f, -0.04775f, 0.8706541f, -4.229002f, 1.599579f, -1.767147f, 3.516372f, 2.451476f, 3.042427f, 1.631346f, -1.131331f, -1.77049f, 3.649992f, -2.759383f, -3.593974f, -3.156021f, -1.026309f, 3.548923f, 0.453126f, 3.466476f, 4.233248f, 3.964321f, 1.728623f, 3.353568f, 2.137495f, 0.4396148f };
        //zitter[4] = new float[36] { 0.7058911f, 4.699615f, 2.724941f, -1.036772f, 1.07246f, 2.952871f, -4.652634f, 4.224998f, 4.515122f, 4.573359f, 1.55325f, -0.04775f, 0.8706541f, -4.229002f, 1.599579f, -1.767147f, 3.516372f, 2.451476f, 3.042427f, 1.631346f, -1.131331f, -1.77049f, 3.649992f, -2.759383f, -3.593974f, -3.156021f, -1.026309f, 3.548923f, 0.453126f, 3.466476f, 4.233248f, 3.964321f, 1.728623f, 3.353568f, 2.137495f, 0.4396148f };
        //zitter[5] = new float[36] { 0.7058911f, 4.699615f, 2.724941f, -1.036772f, 1.07246f, 2.952871f, -4.652634f, 4.224998f, 4.515122f, 4.573359f, 1.55325f, -0.04775f, 0.8706541f, -4.229002f, 1.599579f, -1.767147f, 3.516372f, 2.451476f, 3.042427f, 1.631346f, -1.131331f, -1.77049f, 3.649992f, -2.759383f, -3.593974f, -3.156021f, -1.026309f, 3.548923f, 0.453126f, 3.466476f, 4.233248f, 3.964321f, 1.728623f, 3.353568f, 2.137495f, 0.4396148f };
        //zitter[6] = new float[36] { 0.7058911f, 4.699615f, 2.724941f, -1.036772f, 1.07246f, 2.952871f, -4.652634f, 4.224998f, 4.515122f, 4.573359f, 1.55325f, -0.04775f, 0.8706541f, -4.229002f, 1.599579f, -1.767147f, 3.516372f, 2.451476f, 3.042427f, 1.631346f, -1.131331f, -1.77049f, 3.649992f, -2.759383f, -3.593974f, -3.156021f, -1.026309f, 3.548923f, 0.453126f, 3.466476f, 4.233248f, 3.964321f, 1.728623f, 3.353568f, 2.137495f, 0.4396148f };
        //zitter[7] = new float[36] { 0.7058911f, 4.699615f, 2.724941f, -1.036772f, 1.07246f, 2.952871f, -4.652634f, 4.224998f, 4.515122f, 4.573359f, 1.55325f, -0.04775f, 0.8706541f, -4.229002f, 1.599579f, -1.767147f, 3.516372f, 2.451476f, 3.042427f, 1.631346f, -1.131331f, -1.77049f, 3.649992f, -2.759383f, -3.593974f, -3.156021f, -1.026309f, 3.548923f, 0.453126f, 3.466476f, 4.233248f, 3.964321f, 1.728623f, 3.353568f, 2.137495f, 0.4396148f };
        //zitter[8] = new float[36] { 0.7058911f, 4.699615f, 2.724941f, -1.036772f, 1.07246f, 2.952871f, -4.652634f, 4.224998f, 4.515122f, 4.573359f, 1.55325f, -0.04775f, 0.8706541f, -4.229002f, 1.599579f, -1.767147f, 3.516372f, 2.451476f, 3.042427f, 1.631346f, -1.131331f, -1.77049f, 3.649992f, -2.759383f, -3.593974f, -3.156021f, -1.026309f, 3.548923f, 0.453126f, 3.466476f, 4.233248f, 3.964321f, 1.728623f, 3.353568f, 2.137495f, 0.4396148f };
        //zitter[9] = new float[36] { 0.7058911f, 4.699615f, 2.724941f, -1.036772f, 1.07246f, 2.952871f, -4.652634f, 4.224998f, 4.515122f, 4.573359f, 1.55325f, -0.04775f, 0.8706541f, -4.229002f, 1.599579f, -1.767147f, 3.516372f, 2.451476f, 3.042427f, 1.631346f, -1.131331f, -1.77049f, 3.649992f, -2.759383f, -3.593974f, -3.156021f, -1.026309f, 3.548923f, 0.453126f, 3.466476f, 4.233248f, 3.964321f, 1.728623f, 3.353568f, 2.137495f, 0.4396148f };
        //zitter[10] = new float[36] { 0.7058911f, 4.699615f, 2.724941f, -1.036772f, 1.07246f, 2.952871f, -4.652634f, 4.224998f, 4.515122f, 4.573359f, 1.55325f, -0.04775f, 0.8706541f, -4.229002f, 1.599579f, -1.767147f, 3.516372f, 2.451476f, 3.042427f, 1.631346f, -1.131331f, -1.77049f, 3.649992f, -2.759383f, -3.593974f, -3.156021f, -1.026309f, 3.548923f, 0.453126f, 3.466476f, 4.233248f, 3.964321f, 1.728623f, 3.353568f, 2.137495f, 0.4396148f };
        //zitter[11] = new float[36] { 0.7058911f, 4.699615f, 2.724941f, -1.036772f, 1.07246f, 2.952871f, -4.652634f, 4.224998f, 4.515122f, 4.573359f, 1.55325f, -0.04775f, 0.8706541f, -4.229002f, 1.599579f, -1.767147f, 3.516372f, 2.451476f, 3.042427f, 1.631346f, -1.131331f, -1.77049f, 3.649992f, -2.759383f, -3.593974f, -3.156021f, -1.026309f, 3.548923f, 0.453126f, 3.466476f, 4.233248f, 3.964321f, 1.728623f, 3.353568f, 2.137495f, 0.4396148f };
        //zitter[12] = new float[36] { 0.7058911f, 4.699615f, 2.724941f, -1.036772f, 1.07246f, 2.952871f, -4.652634f, 4.224998f, 4.515122f, 4.573359f, 1.55325f, -0.04775f, 0.8706541f, -4.229002f, 1.599579f, -1.767147f, 3.516372f, 2.451476f, 3.042427f, 1.631346f, -1.131331f, -1.77049f, 3.649992f, -2.759383f, -3.593974f, -3.156021f, -1.026309f, 3.548923f, 0.453126f, 3.466476f, 4.233248f, 3.964321f, 1.728623f, 3.353568f, 2.137495f, 0.4396148f };



        /*/////////////////////////////////////////////test
        for (int i = 0; i < 6; i++)
         {
             for (int j = 0; j < 36; j++)
             {
                 arraysame[i][j] =  j;
             }
         }
         for (int i = 6; i < 12; i++)
         {
             for (int j = 0; j < 36; j++)
             {
                 arraysame[i][j] = 144 - j;
             }
         }
         *////////////////////////////////////////////////
           //記録

        if (blockcount == 1)
        {
            textSave(filePath1, DateTime.Now.ToString("yyyy年MM月dd日hh時mm分"));
            textSave(filePath2, DateTime.Now.ToString("yyyy年MM月dd日hh時mm分"));
            textSave(filePath3, DateTime.Now.ToString("yyyy年MM月dd日hh時mm分"));
            textSave(filePath4, DateTime.Now.ToString("yyyy年MM月dd日hh時mm分"));

            for (int i = 0; i < sametrialnum; i++)
            {
                textSave1(filePath1, "arraysame[" + i.ToString() + "] = new int[36] {");
                for (int j = 0; j < objLnum; j++) textSave1(filePath1, arraysame[i][j].ToString() + ", ");
                textSave(filePath1, arraysame[i][objLnum].ToString() + "};");
            }
            textSave1(filePath1, "arrayT = new int []{");
            for (int j = 0; j < 11; j++) textSave1(filePath1, arrayT[j].ToString() + ", ");
            textSave(filePath1, arrayT[11].ToString() + "};");

            for (int i = 0; i < trialnum; i++)
            {
                textSave1(filePath1, "zitter[" + i.ToString() + "] = new float[36] {");
                for (int j = 0; j < objLnum; j++) textSave1(filePath1, zitter[i][j].ToString() + "f, ");
                textSave(filePath1, zitter[i][objLnum].ToString() + "f};");
            }

            textSave(filePath1, "block\t回数\t入力\t成否\t新旧\ttpos\t試行時間");
        }
        if (transferflag) tmpobj = fix3Prefab;
        else tmpobj = fixPrefab;
        GameObject g = Instantiate(tmpobj, fix.transform);

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        
    }

    // Update is called once per frame
    void Update()
    {
        if (stopflag)
        {
            if (tmp == 1)
            {
                RenderSettings.skybox = skyboxb;
                trial = false;
                //Destroyfix = true;
                tmp = 0;
            }
            if (Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                RenderSettings.skybox = skyboxg;
                Destroyfix = false;
                if (transferflag) tmpobj = fix3Prefab;
                else tmpobj = fixPrefab;
                GameObject g = Instantiate(tmpobj, fix.transform);
                stopflag = false;
            }
        }
        else
        {
            if (trial == false)
            {
                
                //if (Input.GetKeyDown(KeyCode.Keypad5))
                if (Input.GetMouseButtonUp(2))//0:left 1:right 2;middle
                {
                    DestroyL = false;
                    DestroyT = false;
                    Destroyfix = true;
                    //lag = Random.Range(0.5f, 1.5f);
                    lag = 0;
                    lagflag = true;
                    if (transferflag)//true
                    {
                        DestroyArrow = false;
                        Destroyfix2 = false;
                        if (order[trialcount] % 4 == 0)//arrow rightここから
                        {
                            GameObject g = Instantiate(arrowPrefab, fix.transform);
                            GameObject g2 = Instantiate(fix2Prefab, fix.transform);
                            Vector3 transfixpos = new Vector3(6.928f, 0f, -4f);
                            g2.transform.position = transfixpos;
                            transnum = 1;
                        }
                        else if (order[trialcount] % 4 == 1)//arrow left
                        {
                            GameObject g = Instantiate(arrowPrefab, fix.transform);
                            GameObject g2 = Instantiate(fix2Prefab, fix.transform);
                            Vector3 transfixpos = new Vector3(-6.928f, 0f, -4f);
                            Vector3 transarrpos = new Vector3(0.3f, -0.4f, 8f);
                            g2.transform.position = transfixpos;
                            g.transform.Rotate(new Vector3(0f, 0f, 1f), 180);
                            g.transform.position = transarrpos;
                            transnum = 1;
                        }//ここまでコピーして変更しました
                        else //%4 == 2 => stimuli move right  ,   %4 == 3 => stimuli move left
                        {
                            GameObject g = Instantiate(fix2Prefab, fix.transform);
                            transnum = 1;
                        }
                    }
                }
                if (lagflag == true)
                {
                    if (transnum == 1)
                    {
                        if (Input.GetMouseButtonDown(2))
                        {
                            transnum = 0;
                            DestroyArrow = true;
                            Destroyfix2 = true;
                        }
                    }
                    else timelag += Time.deltaTime;
                }
                if (timelag > lag)
                {
                    lagflag = false;
                    pushR = false;
                    pushL = false;
                    timetrial = 0f;
                    //ブロックの更新
                    if (trialcount == trialnum)
                    {
                        trialcount = 0;
                        arrayShuffle(trialnum, order);
                        blockcount++;
                    }

                    trialcount++;



                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////                 
                    array = new int[gridnum];//[24*6]
                    if (order[trialcount - 1] >= sametrialnum)        //new試行//order[試行回数２５]＞＝繰り返しの試行回数１３
                    {


                        // 配列の初期化　
                        for (int i = 0; i < gridx; i++)
                        {
                            for (int j = 0; j < gridy; j++)
                            {
                                array[i * gridy + j] = i * gridy + j;
                            }
                        }
                        ///0 ～ 5,6～11 ・・・・・～24*6-1
                        ///
                        arrayShuffle(gridnum, array);

                        int x = order[trialcount - 1] - sametrialnum;//old→new?
                        //Tの位置//Tの位置を配列の最初に持ってくる
                        for (int j = 1; j < gridnum; j++)
                        {
                            //if (array[j] > (gridx / (sametrialnum - 1)) * gridy * (x) && array[j] < (gridx / (sametrialnum - 1)) * gridy * (x + 1))
                            if (arrayT[x] == array[j])
                            {
                                array[j] = array[0];
                                array[0] = arrayT[x];
                                break;
                            }
                        }
                        //刺激配置
                        int[] tmparray = new int[objLnum + 1];
                        int[] flagarray = new int[gridnum + 2];
                        int y = 0;
                        int num = (objLnum + 1) / separatenum;//3
                        /*
                        for (int j = 0; j < separatenum; j++)
                        {
                            int xx = 0;
                            for (int k = 1; k < gridnum; k++)
                            {
                                if (flagarray[array[k]] == 0 && array[k] >= j * gridnum / separatenum && array[k] < (j + 1) * gridnum / separatenum)
                                {
                                    tmparray[j * num + xx] = array[k];
                                    flagarray[array[k]] = 1;
                                    if (array[k] != 0) flagarray[array[k] - 1] = 1;
                                    flagarray[array[k] + 1] = 1;
                                    xx++;
                                }
                                if (xx == num) break;
                                if (k == gridnum - 1) Debug.Log("aaaaaaaaaaaaaa");
                            }
                        }
                        */


                        /*array[]の定義の仕方
                            １　２　３　４　５・・・・・24
                            ＿＿＿＿＿＿＿＿＿・・・・・＿
                        １| 0   6                       23*6+1
                        ２| 1   7                       23*6+2
                        ・
                        ・
                        ６| 5   11                      23*6+5=24*6-1
                     

                        left:０～４７ center:４８～９５　right:９６～１４３
                         */

                        for (int j = 0; j < separatenum; j++)
                        {
                            int xx = 0;
                            int[] kkk = new int[4] { 0, 0, 0, 0 };
                            for (int k = 1; k < gridnum; k++)
                            {

                                if (flagarray[array[k]] == 0 && array[k] >= j * gridnum / separatenum && array[k] < (j + 1) * gridnum / separatenum)
                                {
                                    int tmpx = array[k] / gridy;
                                    int tmpy = array[k] % gridy;
                                    int tmplocation; //display毎の位置
                                    if (tmpy < gridy / 2)//上半分
                                    {
                                        if (tmpx % grideverydis < grideverydis / 2) tmplocation = 0;//左半分
                                        else tmplocation = 1;
                                    }
                                    else
                                    {
                                        if (tmpx % grideverydis < grideverydis / 2) tmplocation = 2;//左半分
                                        else tmplocation = 3;
                                    }
                                    if (kkk[tmplocation] == 0)
                                    {
                                        kkk[tmplocation] = 1;
                                        tmparray[j * num + xx] = array[k];
                                        flagarray[array[k]] = 1;
                                        if (array[k] != 0) flagarray[array[k] - 1] = 1;
                                        flagarray[array[k] + 1] = 1;
                                        xx++;
                                    }
                                    if (xx == 4) break;
                                }
                            }
                            for (int k = 1; k < gridnum; k++)
                            {

                                if (flagarray[array[k]] == 0 && array[k] >= j * gridnum / separatenum && array[k] < (j + 1) * gridnum / separatenum)
                                {
                                    tmparray[j * num + xx] = array[k];
                                    flagarray[array[k]] = 1;
                                    if (array[k] != 0) flagarray[array[k] - 1] = 1;
                                    flagarray[array[k] + 1] = 1;
                                    xx++;
                                }
                                if (xx == num) break;
                                if (k == gridnum - 1) Debug.Log("aaaaaaaaaaaaaa");
                            }
                        }
                        y = array[0] / gridx;
                        for (int a = 0; a < objLnum + 1; a++) array[a + 1] = tmparray[a];
                        tmp = array[y * gridy + 1];
                        array[y * 6 + 1] = array[objLnum + 1];
                        array[objLnum + 1] = tmp;
                        //for (int i = 0; i < 35; i++) Debug.Log(array[i]);

                        oldnew = "new" + x.ToString();
                    }
                    else    //old試行
                    {
                        for (int i = 0; i < objLnum + 1; i++) array[i] = arraysame[order[trialcount - 1]][i];
                        oldnew = "old" + order[trialcount - 1].ToString();
                    }



                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    Debug.Log(oldnew);
                    Debug.Log(blockcount.ToString() + "." + trialcount.ToString());
                    Tpos = array[0];
                    //上の配列をもとにオブジェクトLを配置、移動、回転する

                    int TMP = 0;
                    for (int i = 1; i <= objLnum; i++)//objLnum35 回転させる部門？
                    {
                        int tmpaaa = gridnum / 6;//24
                        if (array[i] < tmpaaa) TMP = array[i];
                        else if (array[i] < tmpaaa * 2) TMP = array[i] + tmpaaa;
                        else if (array[i] < tmpaaa * 3) TMP = array[i] + tmpaaa * 2;
                        else continue;


                        GameObject g = Instantiate(textLPrefab, textL.transform);
                        float angle = 7.5f - 30f + (TMP / gridy) * (360 / gridx) + zitter[order[trialcount - 1]][i];
                        if (transferflag)//この動作がうまくいかない
                        {
                            if (order[trialcount - 1] % 4 == 2) angle += 120f;
                            else if (order[trialcount - 1] % 4 == 3) angle -= 120f;
                        }

                        float height = (TMP % gridy) - (gridy / 2);

                        int rotate = Random.Range(0, 4);
                        g.transform.position = new Vector3(radius * Mathf.Sin(angle * Mathf.Deg2Rad), height * lenVer, radius * Mathf.Cos(angle * Mathf.Deg2Rad));
                        g.transform.Rotate(new Vector3(0f, 1f, 0f), angle);
                        g.transform.Rotate(new Vector3(0f, 0f, 1f), 90 * rotate);
                        //31回目？
                    }


                  
                    //array[]が意味していることを考える
                    //オブジェクトTの配置 array[]の objLnum番目を使用
                    GameObject h = Instantiate(textTPrefab, textT.transform) as GameObject;
                    if (array[0] < 24) TMP = array[0];
                    else if (array[0] < 48) TMP = array[0] + 24;
                    else if (array[0] < 72) TMP = array[0] + 48;
                    float angleT = 7.5f - 30f + (TMP / gridy) * 360 / gridx;
                    float heightT = (TMP % gridy) - (gridy / 2);

                    if (transferflag)
                    {
                        if (order[trialcount - 1] % 4 == 2) angleT += 120f;
                        else if (order[trialcount - 1] % 4 == 3) angleT -= 120f;
                    }//回転操作の処理？

                    int rotateT = Random.Range(0, 2);
                    h.transform.position = new Vector3(radius * Mathf.Sin(angleT * Mathf.Deg2Rad), heightT * lenVer, radius * Mathf.Cos(angleT * Mathf.Deg2Rad));
                    h.transform.Rotate(new Vector3(0f, 1f, 0f), angleT);
                    h.transform.Rotate(new Vector3(0f, 0f, 1f), (180 * rotateT + 90));

                    textSave(filePath2, "trialcount=" + trialcount.ToString() + "\t" + "blockcount=" + blockcount.ToString());
                    textSave(filePath3, "fin");
                    textSave(filePath4, "fin");
                    textSave(filePath3, oldnew + "\r\n" + "trialcount" + "\t" + trialcount.ToString() + "\t" + "blockcount" + "\t" + blockcount.ToString());
                    textSave(filePath4, oldnew + "\t" + "trialcount=" + trialcount.ToString() + "\t" + "blockcount=" + blockcount.ToString());





                    if (rotateT == 1) Tright = false;
                    else Tright = true;
                    trial = true;
                }
            }

            //----
            if (trial == true)
            {
                timelag = 0f;
                timetrial += Time.deltaTime;


                /*マジわかんね*/
                // FOVEのデータ取得（酒井さんのプログラム参考）
                /*Vector3 data = UnityEngine.FoveInterface.GetHMDRotation().eulerAngles;  //HMDの回転具合の取得*/
                //Vector3 data2 = UnityEngine.FoveInterface.GetLeftEyeVector();   //左目の見ているところ?の取得
                //Vector3 data3 = UnityEngine.FoveInterface.GetRightEyeVector();  //右目の見ているところ?の取得//
                Vector3 data = Camera.main.transform.forward.normalized;//HMDの回転具合の取得
                Vector3 data2 = Camera.main.transform.position + Camera.main.transform.TransformDirection(ViveSR.anipal.Eye.Myeye.getRayRight().direction);//右目の見ているところ?の取得
                Vector3 data3 = Camera.main.transform.position + Camera.main.transform.TransformDirection(ViveSR.anipal.Eye.Myeye.getRayLeft().direction);//左目の見ているところ ? の取得

                //Fove.Managed.EFVR_Eye data = UnityEngine.FoveInterface.CheckEyesClosed();	//目の開閉の取得
                //Vector3 sightline = Camera.main.transform.position + Camera.main.transform.TransformDirection(data2);                                                              //Vector3 data = UnityEngine.FoveInterface.GetHMDPosition ();	//HMDの座標の取得
                textSave(filePath2, data.ToString("G") + "\t" + data2.ToString("G") + "\t" + data3.ToString("G") + "\t" + timetrial.ToString() + "秒");  //string型にキャストしtxtファイルに記録
                textSave(filePath3, data2.ToString("G") + "\t" + timetrial.ToString() + "秒");
                textSave(filePath4, data3.ToString("G") + "\t" + timetrial.ToString() + "秒");
                


                ///正誤記録
                //if (Input.GetKeyDown(KeyCode.Keypad4)) pushL = true;
                if (Input.GetMouseButtonDown(0)) pushL = true;
                //if (Input.GetKeyDown(KeyCode.Keypad6)) pushR = true;
                if (Input.GetMouseButtonDown(1)) pushR = true;

                if (pushR == true || pushL == true)
                {
                    DestroyL = true;
                    DestroyT = true;
                    Destroyfix = false;
                    if (pushR == true && Tright == true) textSave(filePath1, blockcount.ToString() + "\t" + trialcount.ToString() + "\tRight" + "\ttrue\t" + oldnew + "\t" + Tpos.ToString() + "\t" + timetrial.ToString());
                    else if (pushR == true && Tright == false) textSave(filePath1, blockcount.ToString() + "\t" + trialcount.ToString() + "\tRight" + "\tfalse\t" + oldnew + "\t" + Tpos.ToString() + "\t" + timetrial.ToString());
                    else if (pushR == false && Tright == true) textSave(filePath1, blockcount.ToString() + "\t" + trialcount.ToString() + "\tLeft" + "\tfalse\t" + oldnew + "\t" + Tpos.ToString() + "\t" + timetrial.ToString());
                    else textSave(filePath1, blockcount.ToString() + "\t" + trialcount.ToString() + "\tLeft" + "\ttrue\t" + oldnew + "\t" + Tpos.ToString() + "\t" + timetrial.ToString());
                    if (blockcount == blocknum && trialcount == trialnum)
                    {
                        recog = true;
                    }
                    if (blockcount % breaknum == 0 && trialcount == trialnum) //休憩
                    {
                        Destroyfix = true;
                        tmp = 1;
                        stopflag = true;
                    }
                    if (blockcount % 2 == 0 && transferflag && trialcount == trialnum) //休憩
                    {
                        Destroyfix = true;
                        tmp = 1;
                        stopflag = true;
                    }
                    if (transferflag) tmpobj = fix3Prefab;
                    else tmpobj = fixPrefab;
                    GameObject g = Instantiate(tmpobj, fix.transform);
                    trial = false;
                }



            }

        }
    }

    // 引数でStringを渡すとテキストを保存する
    public void textSave(string filePath, string txt)
    {
        StreamWriter sw = new StreamWriter(filePath, true); //true=追記 false=上書き
        sw.WriteLine(txt);//改行あり
        sw.Flush();
        sw.Close();
    }

    public void textSave1(string filePath, string txt)
    {
        StreamWriter sw = new StreamWriter(filePath, true); //true=追記 false=上書き
        sw.Write(txt);
        sw.Flush();
        sw.Close();
    }

    //長さnの配列arrayをシャッフルする
    public void arrayShuffle(int n, int[] array)
    {
        while (1 < n)
        {
            n--;
            int k = Random.Range(0, n);
            int tmp = array[k];
            array[k] = array[n];
            array[n] = tmp;
        }
    }
    public void funcA(int min, int max, int index, int len, int[] array,int num)
    {
        bool[] tmparray = new bool[144 + 3];
        array[0] = arrayT[num];
        tmparray[arrayT[num]] = true;
        int tmp;
        int count = 1;
        while (true)
        {
            tmp = Random.Range(min + 1, max + 1);
            if (tmparray[tmp] == false)
            {
                if (tmparray[tmp - 1] == false && tmparray[tmp + 1] == false)
                {
                    tmparray[tmp] = true;
                    array[index + count] = tmp - 1;
                    count++;
                }
            }
            if (count == len) return;
        }
    }
}
