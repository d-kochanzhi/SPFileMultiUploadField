using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;

namespace FileMultiUploadField.Core
{
    public static class Helper
    {
        /// <summary>
        /// Рекурсивный проход по всем контролам на странице
        /// </summary>
        /// <typeparam name="T">Тип</typeparam>
        /// <param name="rootControl">Корневой контрол</param>
        /// <param name="id">ИД скомого элемента</param>
        /// <returns></returns>
        public static T FindControlRecursive<T>(Control rootControl, String id)
      where T : Control
        {
            T retVal = null;
            if (rootControl.HasControls())
            {
                foreach (Control c in rootControl.Controls)
                {
                    if (c.ID == id) return (T)c;
                    retVal = FindControlRecursive<T>(c, id);
                    if (retVal != null) break;
                }
            }
            return retVal;
        }

        /// <summary>
        /// Преобразование Null в строку
        /// </summary>
        /// <param name="obj">Объект</param>
        /// <returns></returns>
        public static string NullToStr(object obj)
        {
            return obj == null ? string.Empty : obj.GetType().Name == "DBNull" ? string.Empty : obj.ToString();
        }

        /// <summary>
        /// Преобразование Null в буленовский тип
        /// </summary>
        /// <param name="obj">Объект</param>
        /// <returns></returns>
        public static bool NullToBool(object obj)
        {
            bool res;
            if (obj == null) return false;
            if (obj.GetType().Name == "DBNull") return false;

            if (bool.TryParse(obj.ToString(), out res))
            {
                return res;
            }
            else
            {
                if (obj.ToString().ToLower().Equals("1"))
                {
                    return true;
                }
                else if (obj.ToString().ToLower().Equals("0"))
                {
                    return false;
                }
            }

            return false;
        }
    }
}
