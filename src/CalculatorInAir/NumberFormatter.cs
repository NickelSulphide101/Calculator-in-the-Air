using System;
using System.Globalization;
using System.Text;

namespace CalculatorInAir
{
    public static class NumberFormatter
    {
        public static string FormatStandard(double val, int decimalPlaces, bool useThousands)
        {
            if (double.IsNaN(val)) return "NaN";
            if (double.IsInfinity(val)) return double.IsPositiveInfinity(val) ? "∞" : "-∞";

            if (decimalPlaces < 0)
            {
                double rounded = Math.Round(val, 12);
                if (useThousands)
                {
                    string[] parts = rounded.ToString("F12", CultureInfo.InvariantCulture).Split('.');
                    if (long.TryParse(parts[0], out long intPart))
                    {
                        string formattedInt = intPart.ToString("#,##0", CultureInfo.InvariantCulture);
                        if (rounded < 0 && !formattedInt.StartsWith("-"))
                        {
                            formattedInt = "-" + formattedInt;
                        }
                        if (parts.Length > 1)
                        {
                            string dec = parts[1].TrimEnd('0');
                            return string.IsNullOrEmpty(dec) ? formattedInt : $"{formattedInt}.{dec}";
                        }
                        return formattedInt;
                    }
                }
                return rounded.ToString("0.############", CultureInfo.InvariantCulture);
            }
            else
            {
                int safeDecimals = Math.Clamp(decimalPlaces, 0, 15);
                double rounded = Math.Round(val, safeDecimals);
                string fmt = safeDecimals == 0 ? (useThousands ? "#,##0" : "0") : (useThousands ? "#,##0." + new string('0', safeDecimals) : "0." + new string('0', safeDecimals));
                return rounded.ToString(fmt, CultureInfo.InvariantCulture);
            }
        }

        public static string FormatTenThousand(double val)
        {
            if (double.IsNaN(val)) return "NaN";
            if (double.IsInfinity(val)) return double.IsPositiveInfinity(val) ? "∞" : "-∞";

            double wanVal = val / 10000.0;
            double rounded = Math.Round(wanVal, 4);
            return rounded.ToString("0.##", CultureInfo.InvariantCulture) + " 万";
        }

        public static string FormatChineseRMB(double val)
        {
            if (double.IsNaN(val) || double.IsInfinity(val)) return "N/A";
            if (val == 0) return "零元整";

            bool isNegative = val < 0;
            val = Math.Abs(val);

            if (val >= 1e15) return "数值过大";

            long integral = (long)Math.Floor(val);
            long decimalVal = (long)Math.Round((val - integral) * 100);
            if (decimalVal >= 100)
            {
                integral += 1;
                decimalVal = 0;
            }

            string[] digits = { "零", "壹", "贰", "叁", "肆", "伍", "陆", "柒", "捌", "玖" };
            string[] units = { "", "拾", "佰", "仟" };
            string[] bigUnits = { "", "万", "亿", "万亿" };

            var sb = new StringBuilder();
            if (isNegative) sb.Append("负");

            if (integral > 0)
            {
                string intStr = integral.ToString();
                int len = intStr.Length;
                bool zeroFlag = false;

                for (int i = 0; i < len; i++)
                {
                    int digit = intStr[i] - '0';
                    int posFromRight = len - 1 - i;
                    int unitPos = posFromRight % 4;
                    int bigUnitPos = posFromRight / 4;

                    if (digit == 0)
                    {
                        zeroFlag = true;
                        if (unitPos == 0 && bigUnitPos > 0)
                        {
                            if (sb.Length > 0 && sb[sb.Length - 1] == '零')
                            {
                                sb.Remove(sb.Length - 1, 1);
                            }
                            sb.Append(bigUnits[bigUnitPos]);
                            zeroFlag = false;
                        }
                    }
                    else
                    {
                        if (zeroFlag)
                        {
                            sb.Append("零");
                            zeroFlag = false;
                        }
                        sb.Append(digits[digit]);
                        sb.Append(units[unitPos]);

                        if (unitPos == 0 && bigUnitPos > 0)
                        {
                            sb.Append(bigUnits[bigUnitPos]);
                        }
                    }
                }
                sb.Append("元");
            }

            int jiao = (int)(decimalVal / 10);
            int fen = (int)(decimalVal % 10);

            if (jiao == 0 && fen == 0)
            {
                sb.Append("整");
            }
            else
            {
                if (jiao > 0)
                {
                    sb.Append(digits[jiao]).Append("角");
                }
                else if (integral > 0 && fen > 0)
                {
                    sb.Append("零");
                }

                if (fen > 0)
                {
                    sb.Append(digits[fen]).Append("分");
                }
            }

            return sb.ToString();
        }
    }
}
