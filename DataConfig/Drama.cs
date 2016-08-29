using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DataConfig
{
    [Serializable]
    public class Drama
    {
        class Music
        {
            string name;
            float startTime;
        }
        class PlayerAnimation
        {
            string name;
            float startTime;
            bool loop;
        }

        class Player
        {
            string id;
            PlayerAnimation[] anims;
        }

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
    }

    // 必须有无参的构造函数

    [Serializable]
    public class Gift
    {
        [Serializable]
        class Reward
        {
            string type;
            string boy_id;
            string girl_id;
            int min_num;
            int max_num;
            int weight;
        }

        int id;
        [OptionalField]
        UInt32 vv2;
        string str;
        Reward[] reward;

        public void WriteLine()
        {
            //Console.WriteLine(id);
            //Console.WriteLine(vv);
            //Console.WriteLine(str);
            //Console.WriteLine(reward);
        }
    }

    

}
