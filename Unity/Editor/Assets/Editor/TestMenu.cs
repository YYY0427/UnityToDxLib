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

    [MenuItem("�e�X�g���j���[/�_�~�[")]
    public static void DummyFunc()
    {
        Debug.Log("�_�~�[");
    }

    [MenuItem("�e�X�g���j���[/���s")]
    public static void TestFunc()
    {
        var rootObject = Selection.activeGameObject;
        if(rootObject == null)
        {
            EditorUtility.DisplayDialog("�z�u�f�[�^���o�͂ł��܂���", "�z�u�f�[�^���o�͂���ɂ́A���[�g�I�u�W�F�N�g��I�����Ă�������", "����");
            return;
        }
        else
        {
            // SaveFilePanel�̓Z�[�u���Ă���Ȃ��B�������Ԃ�����
            // �Z�[�u�_�C�A���O���o���āA���[�U���I�񂾏o�̓t�H���_���Ԃ����
            var path = EditorUtility.SaveFilePanel("�z�u�f�[�^�o��", ".", "Data.pos", "pos");
            var stream = File.Create(path);
            var bw = new BinaryWriter(stream);

            bw.Write(rootObject.transform.childCount);

            // �����̎q�I�u�W�F�N�g���������邩�𒲂ׂĎq���̍��W���o�͂���
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

                // �I�u�W�F�N�g����ׂ�
                bw.Write((byte)objType);

                // �I�u�W�F�N�g��
                bw.Write(gameObject.name);

                // �ʒu���̏o��
                var pos = gameObject.transform.position;
                WriteVector3(bw, pos);

                // ��]�����o��
                var eular = gameObject.transform.eulerAngles;
                WriteVector3(bw, eular);    // �x���@�ŏo��

                if(objType == ObjectType.Mesh)
                {
                    // �g��k�������o��
                    var scale = gameObject.transform.localScale;
                    WriteVector3(bw, scale);

                    // ���b�V���̖��O(�K�w�\���̃��b�V���̏ꍇ�͓���ȏ������K�v)
                    bw.Write(mesh.sharedMesh.name);
                }
                else if(objType == ObjectType.Camera)
                {
                    bw.Write(camera.fieldOfView);   // ��p
                    bw.Write(camera.nearClipPlane); // Near(�߂�)
                    bw.Write(camera.farClipPlane);  // Far(����)
                }
            }

            bw.Close();
            stream.Close(); ;
            Debug.Log("�e�X�g���j���[�̎��s��������܂���");
            EditorUtility.DisplayDialog("�z�u�f�[�^���o�͂���܂���", path + "�ɏo�͂���܂���", "����");
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
