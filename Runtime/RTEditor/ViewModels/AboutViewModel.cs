//This asset was uploaded by https://unityassetcollection.com

using Battlehub.RTCommon;
using UnityEngine;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.ViewModels
{
    [Binding]
    public class AboutViewModel : MonoBehaviour
    {
        [Binding]
        public string Text
        {
            get
            {
                return
                    "本软件为中科传媒科技有限责任公司开发，仅限受邀人员使用。使用教程请参阅演示视频，如有使用问题请联系本公司开发者。\n联系方式：\n电话：13913555451\n邮箱：dongxin@mail.sciencep.com";
            }
        }
    }
}