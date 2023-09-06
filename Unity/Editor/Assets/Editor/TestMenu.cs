using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class TestMenu 
{
    enum ObjectType
    {
        Mesh, 
        Camera, 
        Light
    }
    public static void WriteVector3(BinaryWriter bw, Vector3 vec)
    {
        bw.Write(vec.x);
        bw.Write(vec.y);
        bw.Write(vec.z);
    }

    [MenuItem("テストメニュー/ダミー")]
    public static void DummyFunc()
    {
        Debug.Log("ダミー");
    }

    [MenuItem("テストメニュー/実行")]
    public static void TestFunc()
    {
        var rootObject = Selection.activeGameObject;
        if(rootObject == null)
        {
            EditorUtility.DisplayDialog("配置データを出力できません", "配置データを出力するには、ルートオブジェクトを選択してください", "閉じる");
            return;
        }
        else
        {
            // SaveFilePanelはセーブしてくれない。文字列を返すだけ
            // セーブダイアログを出して、ユーザが選んだ出力フォルダが返される
            var path = EditorUtility.SaveFilePanel("配置データ出力", ".", "Data.pos", "pos");
            var stream = File.Create(path);
            var bw = new BinaryWriter(stream);

            bw.Write(rootObject.transform.childCount);

            // 自分の子オブジェクトがいくつあるかを調べて子供の座標を出力する
            for(int i = 0; i < rootObject.transform.childCount; i++)
            {
                ObjectType objType = ObjectType.Mesh;
                var gameObject = rootObject.transform.GetChild(i);

                var camera = gameObject.GetComponent<Camera>();
                var light = gameObject.GetComponent<Light>();
                var mesh = gameObject.GetComponent<MeshFilter>();

                if (camera != null)
                {
                    objType = ObjectType.Camera;
                }
                else if(light != null)
                {
                    objType = ObjectType.Light;
                }
                else if(mesh != null)
                {
                    objType = ObjectType.Mesh;
                }
                else
                {
                    continue;
                }

                // オブジェクトじゅべつ
                bw.Write((byte)objType);

                // オブジェクト名
                bw.Write(gameObject.name);

                // 位置情報の出力
                var pos = gameObject.transform.position;
                WriteVector3(bw, pos);

                // 回転情報を出力
                var eular = gameObject.transform.eulerAngles;
                WriteVector3(bw, eular);    // 度数法で出力

                if(objType == ObjectType.Mesh)
                {
                    // 拡大縮小情報を出力
                    var scale = gameObject.transform.localScale;
                    WriteVector3(bw, scale);

                    // メッシュの名前(階層構造のメッシュの場合は特殊な処理が必要)
                    bw.Write(mesh.sharedMesh.name);
                }
                else if(objType == ObjectType.Camera)
                {
                    bw.Write(camera.fieldOfView);   // 画角
                    bw.Write(camera.nearClipPlane); // Near(近い)
                    bw.Write(camera.farClipPlane);  // Far(遠い)
                }
            }

            bw.Close();
            stream.Close(); ;
            Debug.Log("テストメニューの実行が押されました");
            EditorUtility.DisplayDialog("配置データが出力されました", path + "に出力されました", "閉じる");
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
