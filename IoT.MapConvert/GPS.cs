using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoT.MapConvert
{
    /// <summary>
    /// gps转转换类
    /// 用例:GPS.WGS2GCJ(23,114)
    /// 地球上同一个地理位置的经纬度，在不同的坐标系中，会有少于偏移，国内目前常见的坐标系主要分为三种：
    /// 地球坐标系——WGS84：常见于 GPS 设备，Google 地图等国际标准的坐标体系。
    /// 火星坐标系——GCJ-02：中国国内使用的被强制加密后的坐标体系，高德坐标就属于该种坐标体系。
    /// 百度坐标系——BD-09：百度地图所使用的坐标体系，是在火星坐标系的基础上又进行了一次加密处理。
    /// 
    /// </summary>
    public static class GPS
    {
        private static Double PI = 3.14159265358979324;
        private static Double X_PI = 3.14159265358979324 * 3000.0 / 180.0;

        /// <summary>
        /// GPS---百度坐标
        /// </summary>
        /// <param name="lat">纬度</param>
        /// <param name="lng">经度</param>
        /// <returns></returns>
        public static Point WGS2BD(double lat, double lng)
        {
            var wgs2gcjR = WGS2GCJ(lat, lng);
            var gcj2bdR = GCJ2BD(wgs2gcjR.Lat, wgs2gcjR.Lng);
            return gcj2bdR;
        }

        /// <summary>
        /// 火星-百度坐标
        /// </summary>
        /// <param name="lat">纬度</param>
        /// <param name="lng">经度</param>
        /// <returns></returns>
        public static Point GCJ2BD(double lat, double lng)
        {
            var x = lng;
            var y = lat;
            var z = Math.Sqrt(x * x + y * y) + 0.00002 * Math.Sin(y * X_PI);
            var theta = Math.Atan2(y, x) + 0.000003 * Math.Cos(x * X_PI);
            var bd_lon = z * Math.Cos(theta) + 0.0065;
            var bd_lat = z * Math.Sin(theta) + 0.006;

            return new Point { Lat = bd_lat, Lng = bd_lon };
        }
        /// <summary>
        /// 百度-火星坐标系
        /// </summary>
        /// <param name="lat">纬度</param>
        /// <param name="lng">经度</param>
        /// <returns></returns>
        public static Point BD2GCJ(double lat, double lng)
        {
            var x = lng - 0.0065;
            var y = lat - 0.006;
            var z = Math.Sqrt(x * x + y * y) - 0.00002 * Math.Sin(y * X_PI);
            var theta = Math.Atan2(y, x) - 0.000003 * Math.Cos(x * X_PI);
            var gg_lon = z * Math.Cos(theta);
            var gg_lat = z * Math.Sin(theta);

            return new Point { Lat = gg_lat, Lng = gg_lon };
        }

        /// <summary>
        /// 地球坐标系
        /// GPS---高德坐标
        /// </summary>
        /// <param name="lat">纬度</param>
        /// <param name="lng">经度</param>
        /// <returns></returns>
        public static Point WGS2GCJ(double lat, double lng)
        {
            if (OutOfChina(lat, lng))
                return new Point { Lat = lat, Lng = lng };

            // Krasovsky 1940
            //
            // a = 6378245.0, 1/f = 298.3
            // b = a * (1 - f)
            // ee = (a^2 - b^2) / a^2;
            var a = 6378245.0; //  a: 卫星椭球坐标投影到平面地图坐标系的投影因子。
            var ee = 0.00669342162296594323; //  ee: 椭球的偏心率。
            var dLat = TransformLat(lng - 105.0, lat - 35.0);
            var dLng = TransformLng(lng - 105.0, lat - 35.0);
            var radLat = lat / 180.0 * PI;
            var magic = Math.Sin(radLat);
            magic = 1 - ee * magic * magic;
            var sqrtMagic = Math.Sqrt(magic);
            dLat = (dLat * 180.0) / ((a * (1 - ee)) / (magic * sqrtMagic) * PI);
            dLng = (dLng * 180.0) / (a / sqrtMagic * Math.Cos(radLat) * PI);

            var mgLat = lat + dLat;
            var mgLon = lng + dLng;

            return new Point { Lat = mgLat, Lng = mgLon };
        }

        #region
        /// <summary>
        /// 判断是否在中国
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        /// <returns></returns>
        private static bool OutOfChina(double lat, double lng)
        {
            if (lng < 72.004 || lng > 137.8347) return true;
            if (lat < 0.8293 || lat > 55.8271) return true;
            return false;
        }

        /// <summary>
        /// 坐标转换
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private static double TransformLat(double x, double y)
        {
            var ret = -100.0 + 2.0 * x + 3.0 * y + 0.2 * y * y + 0.1 * x * y + 0.2 * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * PI) + 20.0 * Math.Sin(2.0 * x * PI)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(y * PI) + 40.0 * Math.Sin(y / 3.0 * PI)) * 2.0 / 3.0;
            ret += (160.0 * Math.Sin(y / 12.0 * PI) + 320 * Math.Sin(y * PI / 30.0)) * 2.0 / 3.0;
            return ret;
        }

        /// <summary>
        /// 坐标转换
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private static double TransformLng(double x, double y)
        {
            var ret = 300.0 + x + 2.0 * y + 0.1 * x * x + 0.1 * x * y + 0.1 * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * PI) + 20.0 * Math.Sin(2.0 * x * PI)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(x * PI) + 40.0 * Math.Sin(x / 3.0 * PI)) * 2.0 / 3.0;
            ret += (150.0 * Math.Sin(x / 12.0 * PI) + 300.0 * Math.Sin(x / 30.0 * PI)) * 2.0 / 3.0;
            return ret;
        }
        #endregion
    }

    /// <summary>
    /// GPS坐标
    /// </summary>
    public class Point
    {
        /// <summary>纬度</summary>
        public double Lat { get; set; }

        /// <summary>经度</summary>
        public double Lng { get; set; }
    }
}
