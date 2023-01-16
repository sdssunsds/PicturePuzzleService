namespace PicturePuzzleService
{
    /// <summary>
    /// 保存图片的委托
    /// </summary>
    /// <param name="picIndex">图片索引</param>
    /// <param name="dataIndex">图片数据索引</param>
    /// <param name="dataLength">图片数据总数量</param>
    /// <param name="id">唯一编号</param>
    /// <param name="imgData">图片数据</param>
    /// <returns>返回是否执行默认的图片存储方法</returns>
    public delegate bool SavePicture(int picIndex, int dataIndex, int dataLength, string id, byte[] imgData);
    /// <summary>
    /// 拼接图片的委托
    /// </summary>
    /// <param name="id">唯一编号</param>
    /// <param name="picIndexs">需要拼接的图片范围</param>
    /// <returns>返回是否执行默认的图片拼接方法</returns>
    public delegate bool PuzzlePicture(string id, params int[] picIndexs);
    public class PictureDelegete
    {
        public static SavePicture SavePictureFunction;
        public static PuzzlePicture PuzzlePictureFunction;
    }
}
