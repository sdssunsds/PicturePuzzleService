using System.ServiceModel;

namespace PicturePuzzleService
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        void AddLog(string log, int type = 6);

        [OperationContract]
        int GetLocation(string id, string picName1, string picName2, string picName3, string robotID, int state);

        [OperationContract]
        void Upload3DData(string parsIndex, int robot, string data, string id, string robotID);

        [OperationContract]
        void UploadImage(int picIndex, int dataIndex, int dataLength, string id, byte[] imgData, string robotID);

        [OperationContract]
        void UploadImage2(string picIndex, int dataIndex, int dataLength, string id, byte[] imgData, string robotID);

        [OperationContract]
        void UploadImage3(string picName, int dataIndex, int dataLength, string id, byte[] imgData, string robotID);

        [OperationContract]
        void UploadComplete(string id, string robotID, int number);

        [OperationContract]
        string UploadPictrue(string parsIndex, int robot, int dataIndex, int dataLength, string id, byte[] imgData, string robotID);

        [OperationContract]
        void UploadParameter(float[] kc, float[] kk, string robotID);
    }
}
