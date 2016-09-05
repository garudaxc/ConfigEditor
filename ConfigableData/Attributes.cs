using System;
using System.Collections.Generic;
using System.Text;

namespace ConfigableData
{
    public class ConfigDataAttribute : System.Attribute
    {
        string comment_ = "";
        public ConfigDataAttribute(string comment)
        {
            comment_ = comment;
        }

        public string Comment
        {
            get
            {
                return comment_;
            }
        }
    }
}
