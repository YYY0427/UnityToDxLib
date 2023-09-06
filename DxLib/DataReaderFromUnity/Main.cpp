#include <DxLib.h>
#include <string>
#include <vector>
#include <cassert>

using namespace std;
struct Vector3
{
	float x, y, z;
};

// 画角, near, far
struct ProjInfo
{
	float fov, nearClip, farClip;
};

union EtcInfo
{
	Vector3 scale;
	ProjInfo proj;
};

enum class ObjectType
{
	mesh,
	camera,
	light
};

struct UnityGameObject
{
	ObjectType type;
	string name;	// 名前
	Vector3 pos;	// 座標
	Vector3 rot;	// 回転
	EtcInfo etc;	// 拡大率
	string meshName;	// メッシュの名前
};

// 配置データ
struct Arrangement
{
	vector<UnityGameObject> data;
};

// 度数法から弧度法へ(180->π)
float RadianFromDegree(float degree)
{
	return DX_PI_F * degree / 180.0f;
}

int WINAPI WinMain(HINSTANCE, HINSTANCE, LPSTR, int)
{
	ChangeWindowMode(true);
	if (DxLib_Init() == -1)
	{
		return -1;
	}

	// 配置データ
	Arrangement arrange;

	auto dataHandle = FileRead_open("./Data.pos");

	// データ数を得る
	int dataNum = 0;

	int result = FileRead_read(&dataNum, sizeof(dataNum), dataHandle);
	assert(result != -1);

	// オブジェクト数ぶん確保
	arrange.data.resize(dataNum);

	// データの読み取り
	for (int i = 0; i < dataNum; i++)
	{
		auto& data = arrange.data[i];

		uint8_t type = 0;
		result = FileRead_read(&type, sizeof(type), dataHandle);
		assert(result != -1);
		data.type = static_cast<ObjectType>(type);

		// 名前の文字列数を得る
		uint8_t nameSize = 0;
		result = FileRead_read(&nameSize, sizeof(nameSize), dataHandle);
		assert(result != -1);

		// 名前そのものを読み取る
		data.name.resize(nameSize);
		result = FileRead_read(data.name.data(), sizeof(char) * nameSize, dataHandle);
		assert(result != -1);

		// 座標データxyzを読む
		result = FileRead_read(&data.pos, sizeof(data.pos), dataHandle);
		assert(result != -1);

		// 回転データxyzを読む
		result = FileRead_read(&data.rot, sizeof(data.rot), dataHandle);
		assert(result != -1);

		if (data.type == ObjectType::light)
		{
			continue;
		}
		// 拡大データxyzを読む(拡縮:カメラの場合は)
		result = FileRead_read(&data.etc, sizeof(data.etc), dataHandle);
		assert(result != -1);

		if (data.type == ObjectType::mesh)
		{
			result = FileRead_read(&nameSize, sizeof(nameSize), dataHandle);
			assert(result != -1);
			data.meshName.resize(nameSize);
			result = FileRead_read(data.meshName.data(), sizeof(char) * nameSize, dataHandle);
			assert(result != -1);
		}
	}
	FileRead_close(dataHandle);

	// 度数法を弧度法に変換します
	for (auto& object : arrange.data)
	{
		object.rot.x = RadianFromDegree(object.rot.x);
		object.rot.y = RadianFromDegree(object.rot.y);
		object.rot.z = RadianFromDegree(object.rot.z);

		if (object.type == ObjectType::camera)
		{
			object.etc.proj.fov = RadianFromDegree(object.etc.proj.fov);
		}
	}

	SetDrawScreen(DX_SCREEN_BACK);
	while (ProcessMessage() != -1)
	{
		ClearDrawScreen();

		int x = 10, y = 10;
		for (const auto& data : arrange.data)
		{
			DrawFormatString(x, y, 0xffffff, "%s = pos {%.2f, %.2f, %.2f}, rot {%.2f, %.2f, %.2f}", data.name.c_str(), data.pos.x, data.pos.y, data.pos.z, data.rot.x, data.rot.y, data.rot.z);
			y += 16;
			if (data.type == ObjectType::mesh)
			{
				DrawFormatString(x + 100, y, 0xffffff, "scale {%.2f, %.2f, %.2f}, meshName = %s", data.etc.scale.x, data.etc.scale.y, data.etc.scale.z, data.meshName.c_str());
			}
			else if(data.type == ObjectType::camera)
			{
				DrawFormatString(x + 100, y, 0xffffff, "fov = %.2f, near = %.2f, far = %.2f", data.etc.proj.fov, data.etc.proj.nearClip, data.etc.proj.farClip);
			}
			else
			{
				y += 24;
				continue;
			}
			y += 24;
		}
		ScreenFlip();
	}
	DxLib_End();
	return 0;
}