using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageRecognizationTest
{
    public struct WashTagInfo
    {
        public String Description;
        public int CategoryNo;
        public int No;
    }

    public class WashTagDictionary : Dictionary<String, WashTagInfo>
    {
        public WashTagDictionary()
        {
            this.Add("101", new WashTagInfo() { CategoryNo = 1, No = 1, Description = "液温は、95℃を限度とし、洗濯が出来る。" });
            this.Add("102", new WashTagInfo() { CategoryNo = 1, No = 2, Description = "液温は、60℃を限度とし、洗濯機による洗濯が出来る。"});
            this.Add("103", new WashTagInfo() { CategoryNo = 1, No = 3, Description = "液温は、40℃を限度とし、洗濯機による洗濯が出来る。"});
            this.Add("104", new WashTagInfo() { CategoryNo = 1, No = 4, Description = "液温は、40℃を限度とし、洗濯機の弱水流又は弱い手洗い（振り洗い、押し洗い及びつかみ洗い）がよい。"});
            this.Add("105", new WashTagInfo() { CategoryNo = 1, No = 5, Description = "液温は、30℃を限度とし、洗濯機の弱水流又は弱い手洗い（振り洗い、押し洗い及びつかみ洗いがある）がよい。"});
            this.Add("106", new WashTagInfo() { CategoryNo = 1, No = 6, Description = "液温は、30℃を限度とし、弱い手洗い（振り洗い、押し洗い及びつかみ洗いがある）がよい。（洗濯機は使用できない。）"});
            this.Add("107", new WashTagInfo() { CategoryNo = 1, No = 7, Description = "水洗いはできない。"});

            this.Add("201", new WashTagInfo(){ CategoryNo = 2, No = 1, Description = "塩素系漂白剤による漂白ができる。"});
            this.Add("202", new WashTagInfo(){ CategoryNo = 2, No = 2, Description = "塩素系漂白剤による漂白はできない。"});

            this.Add("301", new WashTagInfo() { CategoryNo = 3, No = 1, Description = "アイロンは210℃を限度とし、高い温度（180～210℃まで）で掛けるのがよい。"});
            this.Add("302", new WashTagInfo() { CategoryNo = 3, No = 2, Description = "アイロンは160℃を限度とし、中程度の温度（140～160℃まで）で掛けるのがよい。"});
            this.Add("303", new WashTagInfo() { CategoryNo = 3, No = 3, Description = "アイロンは120℃を限度とし、低い温度（80～120℃まで）で掛けるのがよい。"});
            this.Add("304", new WashTagInfo() { CategoryNo = 3, No = 4, Description = "アイロン掛けはできない。"});

            this.Add("401", new WashTagInfo() { CategoryNo = 4, No = 1, Description = "ドライクリーニングができる。溶剤は､パークロロエチレン又は石油系のものを使用する。"});
            this.Add("402", new WashTagInfo() { CategoryNo = 4, No = 2, Description = "ドライクリーニングができる。溶剤は､石油系のものを使用する。"});
            this.Add("403", new WashTagInfo() { CategoryNo = 4, No = 3, Description = "ドライクリーニングはできない。"});

            this.Add("501", new WashTagInfo() { CategoryNo = 5, No = 1, Description = "手絞りの場合は弱く、遠心脱水の場合は、短時間で絞るのがよい。"});
            this.Add("502", new WashTagInfo() { CategoryNo = 5, No = 2, Description = "絞ってはいけない。"});

            this.Add("601", new WashTagInfo() { CategoryNo = 6, No = 1, Description = "つり干しがよい。"});
            this.Add("602", new WashTagInfo() { CategoryNo = 6, No = 2, Description = "日陰のつり干しがよい。"});
            this.Add("603", new WashTagInfo() { CategoryNo = 6, No = 3, Description = "平干しがよい。" });
            this.Add("604", new WashTagInfo() { CategoryNo = 6, No = 4, Description = "日陰の平干しがよい。" });
        }
    }


    public class WashTagGroupDictionary : Dictionary<int, int[]>
    {
        public WashTagGroupDictionary()
        {
            this.Add(100, new int[] { 101, 102, 103, 104, 105 });
            this.Add(106, new int[] { 106 });
            this.Add(107, new int[] { 107 });

            this.Add(201, new int[] { 201 });
            this.Add(202, new int[] { 202 });

            this.Add(300, new int[] { 301, 302, 303 });
            this.Add(304, new int[] { 304 });

            this.Add(401, new int[] { 401, 402 });
            this.Add(403, new int[] { 403 });

            this.Add(501, new int[] { 501 });
            this.Add(502, new int[] { 502 });

            this.Add(601, new int[] { 601, 603 });
            this.Add(602, new int[] { 602, 604 });            
        }
    }
}
