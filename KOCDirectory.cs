using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class KOCDirectory : MonoBehaviour
{
    KOC OrgKoc; //原始KOC
    List<KOC> KocDirectory;     //koc文件夹
    List<KOC> More5;    //相似度大于0.5
    List<KOC> Less5;    //相似度小于0.5

    public KOCDirectory(List<KOC> KocDirectory)
    {
        this.KocDirectory = KocDirectory;
        GetKocArray();
    }

    public void GetKocArray()
    {
        this.More5 = new List<KOC>();
        this.Less5 = new List<KOC>();
        foreach (var item in KocDirectory)
        {
            if ((item.simply)!="0")
            {
                More5.Add(item);

            }
            else
            {
                Less5.Add(item);
            }
        }

    }
    
}
