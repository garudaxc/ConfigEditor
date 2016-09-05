using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DataConfig
{


    [ConfigableData.ConfigData("动画脚本")]
    [Serializable]
    public class Drama
    {
        [Serializable]
        class Music
        {
            string name;
            float startTime;
        }
        [Serializable]
        class PlayerAnimation
        {
            string name;
            float startTime;
            bool loop;
        }

        [Serializable]
        class Player
        {
            string id;
            PlayerAnimation[] anims;
        }

        [Serializable]
        class Dialogue
        {
            string speakerId;
            string content;
        }

        string bgImage;
        Music bgMusic;
        Player leftPlayer;
        Player rightPlayer;
        Dialogue dialogue;
        int aaa;
    }

    // 必须有无参的构造函数

    [Serializable]
    public enum GiftType
    {
        Money,
        Item,
        Other,
    }


    [ConfigableData.ConfigData("奖励")]
    [Serializable]
    public class Gift
    {
        [Serializable]
        class Reward
        {
            string type;
            string boy_id = "001";
            string girl_id = "002";
            int min_num;
            int max_num = 99;
            int weight;
        }

        class Dialogue
        {
            string speakerId = "001";
            string content;
        }

        int id;

        GiftType type;

        bool isLoop = false;
        //[OptionalField]
        //UInt32 vv2;
        //string str;   
        Dialogue dialogue;
        Reward[] reward;

        public static Gift LoadFrom(string path)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(Gift));
            FileStream stream = new FileStream(path, FileMode.Open);
            Gift value = (Gift)serializer.ReadObject(stream);

            return value;
        }


        public void WriteLine()
        {
            //Console.WriteLine(id);
            //Console.WriteLine(vv);
            //Console.WriteLine(str);
            //Console.WriteLine(reward);
        }
    }



}
