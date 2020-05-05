using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Text;


public class Control : MonoBehaviour
{
    public GameObject LeftUI;
    public GameObject RightUI;
    private int nums;       //待播放koc库里的数量
    private int index;      //播放sps里的第几张
    public float speed;     //速度
    float f1;           //播放每张图片的倒计时
    bool play;          //控制是否播放
    //float i;
    
    public GameObject Screen_play;          //屏幕
    Text ScreenText;  //播放屏幕的图片对应的字符串
    private string assetPath;                                       //当前Asset目录 
    private string imagesPath;                                      //image所在的目录   


    //大屏幕所需
    private List<Sprite> sps;       //播放列表      准备播放的koc Sprite
    private Dictionary<string, string> sps_text;    //koc 和其对应对相似度 
    List<Sprite> sps_all; ////点击左边UI名字后koc对应的koc库里的所有图片


    //右边展示所需
    List<Sprite> sps_Right_ing;     //右边UI当前显示的图片
    List<Sprite> sps_Right_used;    //大于0.5的
    List<Sprite> sps_Right_unused;  //小于0.5的
    //左边展示所需
    List<string> WaitTestKocName;       //左边待测试KOC的名字
    List<Sprite> sps_Left_WaitTestKoc; //左边待测试KOC

    Dictionary<string, List<string>> dsinfo;                 //储存文件夹名及其里面的jpg,和config名
    Dictionary<string, List<string>> configs;               //储存文件夹名==》相似度列表

    Dictionary<string, List<Sprite>> AllKocNameSprites;      //总库，里面包含所有的图片，无论是相似度是1还是0的,有些可能没在相似度列表里出现的
                                                             //Dictionary<string, List<Dictionary<string, string>>> usedKocSimeply;    //通过名字获取对应的koc 相似度对应表  

    Dictionary<string, Dictionary<string, string>> kocsim;      //待匹配koc对应的名字和其相似度
    Dictionary<string, List<Sprite>> needkoc;       //待匹配的koc的名字和其对应的Sprite //传名字

    
    void Awake()
    {
       
        assetPath = Application.dataPath;
        imagesPath = Path.Combine(assetPath, "Resources/images");   //images路径
        Debug.Log("左边UI名：" + LeftUI.name);
        dsinfo = new Dictionary<string, List<string>>();    //初始化名字，图片字典集合
        configs = new Dictionary<string, List<string>>();   //初始化名字:相似度列表集合
        WaitTestKocName = new List<string>();
        string[] ImagesPathDiectory = GetDirectory(imagesPath);     //有几个文件夹

        foreach (var item in ImagesPathDiectory)
        {
            //Debug.Log(item);
            string[] s = item.Split('\\');  //将Resources/images里每个文件夹的路径进行Split，取得文件名
            var h_name = s[s.Length - 1];   //获取文件夹名    单个koc的
            WaitTestKocName.Add(h_name);
            //Debug.Log(h_name);
            List<string> imagefiles = _Getfils(item, ".jpg");     //返回jpg图片图路径
            List<string> config = _Getfils(item, ".txt");                    //返回相似度列表
           
            configs[h_name] = config;       //60592:["60592_rlt_log_0421.txt","60592_rlt_log_0421.txt"]
            dsinfo[h_name] = imagefiles;
        }
        Debug.Log("有"+dsinfo.Count+"个待测试KOC");

        AllKocNameSprites = getinfo();  //获取Resources/images里所有的图片为Sprite存起来
                                       

       
        sps_Right_unused = new List<Sprite>();




    }
    void Start()
    {
       
        sps_all = AllKocNameSprites[WaitTestKocName[0]];                //第一个文件夹里全部的koc图片

        GetKocSimiler(ref sps_all, out kocsim, out needkoc);            //更新kocsim(文件夹名和其对应的koc，相似度列表) 和needkoc
        sps_text = kocsim[WaitTestKocName[0]];                          //更新单个koc其相似度列表
        sps = needkoc[WaitTestKocName[0]];                              //更新播放列表
        GetUnusedSprite();                                              //更新没使用的图片 sps_Right_unused        
        sps_Right_used = sps;                                           //更新相似度大于0.5的图片
        sps_Right_ing = sps;                                            //默认开始时将播放列表中的显示出来
        CreatLeftUI();                                                  //初始化左边UI
        CreatRightUI();                                                 //创建
        TheFirstPlayKoc();                                              //默认点播放时播放第一个待测试koc


        index = 0;
        f1 = speed;
        play = false;


    }

    // Update is called once per frame
    void Update()
    {

        if (play)
        {
            Play();

        }


        //退出游戏
        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }

    }

    //=========================点击事件=======================================
    //放在updata里，真正的播放动作
    void Play()
    {

        f1 -= Time.deltaTime;
        //Debug.Log(f1);
        if (f1 < 0)
        {
            T1();
            f1 = speed;

        }
    }
    //点播放时
    public void Play2()
    {
        index = 0;
        play = true;
        f1 = speed;
    }
    public void Stop()
    {
        play = false;
        f1 = speed;
    }
    public void Replay()
    {
        index = 0;
        play = true;
        f1 = speed;

    }
    public void Next()
    {
        //暂停
        play = false;
        f1 = speed;

        Debug.Log(index);
        if (index >= sps.Count - 1)
        {
            index = 0;
        }
        index += 1;
        ChangeScreenInfo(index);


    }
    public void Laxt()
    {
        //暂停
        play = false;
        f1 = speed;
        Debug.Log(index);
        if (index <= 0)
        {
            index = sps.Count - 1;
        }
        index -= 1;
        ChangeScreenInfo(index);
    }
    //点击按扭，获取相应的名字，将播放列表切换至相就的list
    /// <summary>
    /// 控制左边待测试列表
    /// </summary>
    public void GetKocName()
    {


        //sps = null;
        //sps_text = null;
        //nums = 0;
        index = 0;  //重置播放为第一张 
        var buttonSelf = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;        //被点击按钮自身
        //Debug.Log(buttonSelf);
        var texturename = buttonSelf.transform.GetChild(1).gameObject.GetComponent<Text>().text;
        //Debug.Log(texturename);
        var WaitTestKOC = buttonSelf.transform.GetChild(0).gameObject.GetComponent<Image>().sprite;     //将待测试KOC的Sprite传入屏幕，
        //Debug.Log(WaitTestKOC);
        Screen_play.transform.GetComponent<Image>().sprite = WaitTestKOC;   //当点击时屏幕KOC图片使用被点击的待测试KOC图片
        Screen_play.transform.GetChild(0).GetComponent<Text>().text = "";
        sps_all = AllKocNameSprites[texturename];   //当前名字的koc对应的koc库里的所有图片
        // 通过这个GetKocSimiler，从sps_all这个待测Koc总库里获取对应的koc相似度和已经入选0.5以内相似度的koc列表

        GetKocSimiler(ref sps_all, out kocsim, out needkoc);

        sps = needkoc[texturename]; //更新播放列表  

        //Debug.Log(sps.Count.ToString() + "个Sprite");



        //sps_text = kocsim[texturename];   //返回这个koc名取到对应的koc相似度字典            
        bool c = kocsim.TryGetValue(texturename, out sps_text);
        if (!c)
        {
            Debug.Log(string.Format("没有找到{0}对应的koc相似度列表", texturename));
        }
        nums = sps.Count;

        GetUnusedSprite();  //更新没用的列表    
        sps_Right_used = sps;
        sps_Right_ing = sps;
        CreatRightUI();     //每点击一次更新一下右边的UI


    }

    public void ClickSingle_RightPic()
    {
        var buttonSelf = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        var texturename = buttonSelf.transform.GetChild(1).gameObject.GetComponent<Text>().text;
        //Debug.Log(texturename);
        var WaitTestKOC = buttonSelf.transform.GetChild(0).gameObject.GetComponent<Image>().sprite;     //将待测试KOC的Sprite传入屏幕，
        Debug.Log("所点物体名:"+WaitTestKOC.name);
        string text="";    //准备在屏幕显示的字符串
        

        bool c1 = sps_text.TryGetValue(texturename, out text);
       
        if (!c1)
        {
            text = texturename;
            Debug.Log("相似度:"+text);
        }
        Screen_play.transform.GetComponent<Image>().sprite = WaitTestKOC;   //当点击时屏幕KOC图片使用被点击的待测试KOC图片
        Screen_play.transform.GetChild(0).GetComponent<Text>().text = text;
    }

    public void More_5()
    {
        
        sps_Right_ing = sps_Right_used;
        //Debug.Log("大于0.5的"+sps_Right_ing[0]);
       
        CreatRightUI();
    }
    public void Less_5()
    {
       
        sps_Right_ing = sps_Right_unused;
        //Debug.Log("小于0.5的"+sps_Right_ing[0]);
        
        CreatRightUI();
    }
    
    //==================================工具方法=====================================================
    /// <summary>
    /// 默认第一次点播放时播放的内容 
    /// </summary>
    void TheFirstPlayKoc()
    {
        GameObject Pic;
        if (GameObject.Find("Left_Content").transform.childCount<1)
        {
            Debug.Log("没有待测试文件");
        }
        Pic = GameObject.Find("Left_Content").transform.GetChild(1).gameObject;
        Debug.Log(Pic.name);
        if (Pic == null)
        {
            Debug.Log("待测试列表为空,请检查Left_Content是否有内容!");
        }
        var texturename = Pic.transform.GetChild(1).gameObject.GetComponent<Text>().text;

        var WaitTestKOC = Pic.transform.GetChild(0).gameObject.GetComponent<Image>().sprite;     //将待测试KOC的Sprite传入屏幕，

        Screen_play.transform.GetComponent<Image>().sprite = WaitTestKOC;   //当点击时屏幕KOC图片使用被点击的待测试KOC图片

        var sps_all = AllKocNameSprites[texturename];   //更新播放列表   
        // 通过这个GetKocSimiler，从sps_all这个待测Koc总库里获取对应的koc相似度和已经入选0.5以内相似度的koc列表
        GetKocSimiler(ref sps_all, out kocsim, out needkoc);

        sps = needkoc[texturename];
        Debug.Log(sps.Count.ToString() + "个Sprite");

        //foreach (var item in sps)
        //{
        //    Debug.Log(item);
        //}

        //sps_text = kocsim[texturename];   //返回这个koc名取到对应的koc相似度字典            
        bool c = kocsim.TryGetValue(texturename, out sps_text);
        if (!c)
        {
            Debug.Log(string.Format("没有找到{0}对应的koc相似度列表", texturename));
        }
        nums = sps.Count;
    }
    /// <summary>
    /// 改变屏幕信息的入口
    /// </summary>
    /// <param name="index">待播放的List<Sprite>列表元素的下标,即播放第几张</param>
    void ChangeScreenInfo(int index)
    {
        Screen_play.transform.GetComponent<Image>().sprite = sps[index];    //指定图片
        string ingname = sps[index].name;
        string text;
        bool c1 = sps_text.TryGetValue(ingname, out text);
        if (!c1)
        {
            text = "";
        }
        //Screen_play.transform.GetChild(0).GetComponent<Text>().text = sps_text[ingname].ToString();   //旧
        Screen_play.transform.GetChild(0).GetComponent<Text>().text = text; //新 //为不在选择范围内没有koc相似度字典的做准备
    }
    /// <summary>
    /// 点播放时实际调用的入口
    /// </summary>
    void T1()
    {
        if (index <= sps.Count - 1)
        {

            //指定相似度
            if (sps[index] != null)
            {
                //Screen_play.transform.GetComponent<Image>().sprite = sps[index];    //指定图片
                //string ingname = sps[index].name;
                //Screen_play.transform.GetChild(0).GetComponent<Text>().text = sps_text[ingname].ToString();
                //Debug.Log(sps[index].name);
                ChangeScreenInfo(index);
                index += 1;

            }
            else
            {
                Debug.Log("图片Sprite为空!");
            }


        }

    }

    void OnClick()
    {
        Play();
    }


    #region
    /// <summary>   Path类
    /// 1，GetFileName()方法：从路径字符串中得到文件名(带扩展名)
    /// 2，GetFileNameWithoutExtension()方法，从路径字符串中得到文件名(不带扩展名)。
    /// 3，GetExtension()方法，从文件路径字符串中得到文件的扩展名。
    /// 4,GetDirectoryName()方法，得到文件的文件夹路径。
    /// 5，GetFullPath()方法，从文件字符串中得到包括文件名和扩展名的全路径名。
    /// 6，Combine()方法，合并两个文件路径字符串。
    /// </summary>
    /// <param name="path"></param>
    #endregion
    #region
    ///Directory类
    /// 1.目录创建方法：Directory.CreateDirectory
    /// 2.目录属性设置方法：DirectoryInfo.Atttributes
    /// 3.目录删除方法：Directory.Delete
    /// 4.目录移动方法：Directory.Move
    /// 5.获取当前目录下的所有子目录方法：Directory.GetDirectories
    /// 6.获取当前目录下的所有文件方法：Directory.GetFiles
    /// 7.判断目录是否存在方法：Directory.Exist
    #endregion
    //获取指定目录下的所以子目录
    private string[] GetDirectory(string path)
    {
        string[] Directorys;

        Directorys = Directory.GetDirectories(imagesPath);

        //foreach (var item in Directorys)
        //{
        //    Debug.Log(item);
        //}
        return Directorys;
    }
    /// <summary>
    /// 获取指定目录中指定类型的文件列表
    /// </summary>
    /// <param name="path">文件夹路径</param>
    /// <param name="sub">想要获取哪种类型的文件</param>
    /// <returns>sub类型的文件列表</returns>
    private List<string> _Getfils(string path, string sub)
    {
        string[] Files; //获取到的文件夹里的所有文件，包括.meta文件
        List<string> images = new List<string>();   //收集已经过滤的文件
        Files = Directory.GetFiles(path);
        foreach (var item in Files)
        {
            if (item.EndsWith(sub))
            {
                images.Add(item);
                //Debug.Log(item);
            }
        }
        return images;
    }

    /// <summary>
    /// 获取该路径的图片字节流
    /// </summary>
    /// <param name="imagePath">图片地址</param>
    /// <returns></returns>
    private byte[] getImageByte(string imagePath)
    {
        FileStream files = new FileStream(imagePath, FileMode.Open);
        byte[] imgByte = new byte[files.Length];
        files.Read(imgByte, 0, imgByte.Length);
        files.Close();
        return imgByte;

    }
    /// <summary>
    /// 从该路径读取图片生成Sprite
    /// </summary>
    /// <param name="path">图片具体的路径</param>
    /// <returns>Sprite</returns>
    private Sprite LoadTexture(string path)
    {
        Texture2D t2 = new Texture2D(800, 600);
        t2.LoadImage(getImageByte(path));
        Sprite tsprite = Sprite.Create(t2, new Rect(0, 0, t2.width, t2.height), Vector2.zero);
        //Debug.Log("tsprite" + tsprite);
        //Debug.Log("tsprite 类型" + tsprite);
        tsprite.name = Path.GetFileNameWithoutExtension(path);
        return tsprite;
    }
    /// <summary>
    /// 初始化Resources/images里所有的图片为Sprite
    /// </summary>
    /// <returns>待测试名字及其所以图片</returns>
    public Dictionary<string, List<Sprite>> getinfo()
    {
        Dictionary<string, List<Sprite>> AllKocNameSprites = new Dictionary<string, List<Sprite>>();    //储存名字我其名下的所以Sprite

        foreach (var items in dsinfo.Keys)
        {
            List<Sprite> spe = new List<Sprite>();

            for (int i = 0; i < dsinfo[items].Count; i++)
            {
                var picName = Path.GetFileNameWithoutExtension(dsinfo[items][i]);
                var ResourName = Path.Combine(imagesPath, items, picName+".jpg");
                //string texturepath = Path.Combine()
                //Debug.Log("picName:" + picName);
                //Debug.Log("ResourName:" + ResourName);
               
                //spe.Add(Resources.Load<Sprite>(ResourName));
                spe.Add(LoadTexture(ResourName));
            }
            AllKocNameSprites[items] = spe;
        }
        return AllKocNameSprites;

    }

    /// <summary>
    /// 读取文件，返回所有行
    /// </summary>
    /// <param name="Path">要读取的文件路径</param>
    /// <returns></returns>
    public List<string> ReadTxtContent(string Path)
    {
        List<string> contens = new List<string>();
        StreamReader sr = new StreamReader(Path, Encoding.Default);
        string content;
        while ((content = sr.ReadLine()) != null)
        {
            if (content != "")
            {
                contens.Add(content.ToString());
            }

        }
        return contens;
    }

    /// <summary>
    /// 从图片集合里获取某个图片
    /// </summary>
    /// <param name="refsprite">需要从哪个Sprite里取图片</param>
    /// <param name="name">该图片的名称</param>
    /// <returns></returns>
    private Sprite GetNeedSprite(List<Sprite> refsprite, string name)
    {

        for (int i = 0; i < refsprite.Count; i++)
        {
            if (refsprite[i].name == name)
            {
                return refsprite[i];
            }
        }
        return null;
    }
    /// <summary>
    /// 返回koc 相似度列表
    /// key为对应的原koc名字
    /// value是一个list<>
    /// 这个list里是两个字典
    /// 字典里保存koc 与相似度的两种方式.
    /// </summary>
    /// <returns>返回koc 相似度列表</returns>
    public void GetKocSimiler(ref List<Sprite> AllSprite, out Dictionary<string, Dictionary<string, string>> ds, out Dictionary<string, List<Sprite>> sp)
    {
        //待匹配koc名,List(两个字典)，字典里有每个具体的koc图片和其对应的相似度
        //Dictionary<string, Dictionary<string, string>> kocnamesimaply = new Dictionary<string, Dictionary<string, string>>();
        ds = new Dictionary<string, Dictionary<string, string>>();
        sp = new Dictionary<string, List<Sprite>>();


        //dsinfo Resources image里的文件
        foreach (var items in dsinfo.Keys)
        {
            //items 原始koc相似度列表路径
            //configs[items] --60592文件夹里的.txt文件List
            string infopath;
            if (configs[items].Count <= 0)
            {
                Debug.Log(string.Format("{0}没有相似度列表", items));
                infopath = "";
            }
            infopath = configs[items][0];       //如果操作多个相似度列表，在这里添加
            //Debug.Log("原始koc相似度列表路径" + infopath);       //E:/Unity/YF/Unity_Example/tes1/Assets\Resources/images\823637\823637_rlt_log_0421.txt
            //这个原始koc对应的保存其匹配相似度的列表的txt的地址
            //Debug.Log("============Path=========" + infopath);
            //读取这个文件，返回里面的每行
            List<string> contents = ReadTxtContent(infopath);
            Debug.Log("总共有" + contents.Count.ToString() + "条数据");

            //临时保存koc名和其对应的相似度
            Dictionary<string, string> kocSim = new Dictionary<string, string>();
            List<Sprite> spSprite = new List<Sprite>();

            //切内容,取koc名与相似度，组合成两个字典
            for (int i = 0; i < contents.Count; i++)
            {
                var ss = contents[i].Split();
                //Debug.Log(ss.GetType());
                //Debug.Log(ss[0] + ss[1]);

                kocSim[ss[0]] = ss[1];
                Sprite nsprite = GetNeedSprite(AllSprite, ss[0]);
                //如果不为空，才添加进spSprite
                if (nsprite != null)
                {
                    spSprite.Add(nsprite);
                }


            }


            ds[items] = kocSim;
            sp[items] = spSprite;
        }


    }
    /// <summary>
    /// 用于动态添加物体
    /// </summary>
    /// <param name="_sprite">Sprite</param>
    /// <param name="dst">想要复制哪个物体</param>
    /// <param name="name">想将集合里拿哪个名字的Sprite</param>
    /// <param name="parent">复制出来的物体的父物体是谁</param>
    void AddGameOjbect(Sprite _sprite, GameObject dst, string name, Transform parent)
    {
        //复制
        GameObject go = Instantiate(dst);
        go.SetActive(true);
        //指定父物体
        go.transform.SetParent(parent);
        //Sprite _sprite = GetNeedSprite(needSpriteList, name);

        //获取所有子物体        
        for (int i = 0; i < go.transform.childCount; i++)
        {
            var go1 = go.transform.GetChild(i).gameObject;
            if (go1.name == "Image" && _sprite != null)
            {
                go1.GetComponent<Image>().sprite = _sprite;

            }
            else if (go1.name == "Text" && _sprite != null)
            {
                go1.GetComponent<Text>().text = name;

            }

        }

    }
    /// <summary>
    /// 创建左边UI
    /// </summary>
    void CreatLeftUI()
    {
        Transform parent = GameObject.Find("Left_Content").transform;
        sps_Left_WaitTestKoc = new List<Sprite>();
        for (int i = 0; i < WaitTestKocName.Count; i++)
        {
            //名字
            string _kocname = "";
            _kocname = WaitTestKocName[i];
            //Debug.Log("_kocnam:"+ _kocname);
            //对应的Sprite
            var _koclist = AllKocNameSprites[_kocname]; //获取某文件夹里所有的koclist
            //Debug.Log("_koclis:" + _koclist[0].name);
            //var _kocname2 = AllKocNameSprites[_kocname][0].name; //获取某文件夹里第一个kocSprite的名字
            //Sprite _kocSprite = GetNeedSprite(_koclist, _kocname2);
            Sprite _kocSprite = _koclist[0];
            //Debug.Log("UI:" + _kocSprite.name);
            //添加进左边UI集合
            sps_Left_WaitTestKoc.Add(_kocSprite);
            AddGameOjbect(_kocSprite, LeftUI, _kocSprite.name, parent);
            //Debug.Log("左这UI添加:" + _kocname + "成功");

        }
    }

    //初始化   sps_Right_used ,sps_Right_unused

    void CreatRightUI()
    {


        //Debug.Log("sps_Right_used:"+ sps_Right_used[0]);
        //Debug.Log("sps_Right_ing:" + sps_Right_ing[0]);
        if(GameObject.Find("Right_Content").transform.childCount > 1)
        {
            DestroyChilds(GameObject.Find("Right_Content"));
        }
       
        foreach (var item in sps_Right_ing)
        {

            //名字
            string _kocname = "";
            //Debug.Log("item.name:" + item.name);

            bool c = sps_text.TryGetValue(item.name, out _kocname);

            //Debug.Log("item.name:"+ item.name+"\t"+ _kocname);
            
            //Debug.Log(c);
            if (!c)
            {
                _kocname = item.name;
            }
            //Debug.Log("相似度为:" + _kocname);

            //对应的Sprite
            Transform parent = GameObject.Find("Right_Content").transform;
            Sprite _kocSprite = item;
            //Debug.Log("右边UI:" + _kocSprite.name);
            //添加进左边UI集合            
            AddGameOjbect(_kocSprite, RightUI, _kocname, parent);
            //Debug.Log("右边UI添加:" + _kocname + "成功!");
        }
    }

    void GetUnusedSprite()
    {
        if (sps_Right_unused.Count > 0)
        {
            sps_Right_unused.Clear();
        }

        if (sps_all != null)
        {
            foreach (var item in sps_all)
            {
                //Debug.Log(item);
                //Debug.Log(sps[0]);
                if (!sps.Contains(item))
                {
                    sps_Right_unused.Add(item);
                }
            }
        }
        
    }

    void DestroyChilds(GameObject g)
    {
        var coms = g.GetComponentsInChildren<Transform>();
        for (int i = 1; i < coms.Length; i++)
        {
            Destroy(coms[i].gameObject);
        }
    }
}
