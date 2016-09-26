using System.Collections.Generic;
using System.Reflection;

namespace System.Net
{

    /// <summary>
    /// Contains extensions for the <see cref="CookieContaner"/> class.
    /// </summary>
    public static class CookieContainerExtensions
    {

        /// <summary>
        /// Uses Reflection to get ALL of the <see cref="Cookie">Cookies</see> where <see cref="Cookie.Domain"/> 
        /// contains part of the specified string. Will return cookies for any subdomain, as well as dotted-prefix cookies. 
        /// </summary>
        /// <param name="cookieContainer">The <see cref="CookieContainer"/> to extract the <see cref="Cookie">Cookies</see> from.</param>
        /// <param name="domain">The string that contains part of the domain you want to extract cookies for.</param>
        /// <returns></returns>
        public static IEnumerable<Cookie> GetCookies(this CookieContainer cookieContainer, string domain)
        {
            var domainTable = GetFieldValue<dynamic>(cookieContainer, "_domainTable");
            foreach (var entry in domainTable)
            {
                string key = GetPropertyValue<string>(entry, "Key");

                if (key.Contains(domain))
                {
                    var value = GetPropertyValue<dynamic>(entry, "Value");

                    var internalList = GetFieldValue<SortedList<string, CookieCollection>>(value, "_list");
                    foreach (var li in internalList)
                    {
                        foreach (Cookie cookie in li.Value)
                        {
                            yield return cookie;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the value of a Field for a given object instance.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> you want the value to be converted to when returned.</typeparam>
        /// <param name="instance">The Type instance to extract the Field's data from.</param>
        /// <param name="fieldName">The name of the Field to extract the data from.</param>
        /// <returns></returns>
        internal static T GetFieldValue<T>(object instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo fi = instance.GetType().GetField(fieldName, bindFlags);
            return (T)fi.GetValue(instance);
        }

        /// <summary>
        /// Gets the value of a Property for a given object instance.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> you want the value to be converted to when returned.</typeparam>
        /// <param name="instance">The Type instance to extract the Property's data from.</param>
        /// <param name="propertyName">The name of the Property to extract the data from.</param>
        /// <returns></returns>
        internal static T GetPropertyValue<T>(object instance, string propertyName)
        {
            var pi = instance.GetType().GetProperty(propertyName);
            return (T)pi.GetValue(instance, null);
        }

    }

}