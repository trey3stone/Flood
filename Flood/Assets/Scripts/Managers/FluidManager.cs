﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidManager : MonoSingleton<FluidManager>
{
	private const int THREAD_GROUP_SIZE = 8;

	private ComputeShader fluidSim;
	private SwapBuffer fluidBuffer;

	private int updateKernel;

	private Fluid fluid;

	private int res;

	protected override void DerivedAwake() {
		fluidSim = Resources.Load("Shaders/FluidSim") as ComputeShader;
		updateKernel = fluidSim.FindKernel("Update");
	}

	// Start is called before the first frame update
	void Start() {

	}

	// Update is called once per frame
	void FixedUpdate() {
		if (GameManager.Self.Paused) return;
		//print("UP!");
		// TODO: Move to FixedUpdate?

		fluidBuffer.Swap();

		fluidSim.SetBuffer(updateKernel, "Front", fluidBuffer.Front);
		fluidSim.SetBuffer(updateKernel, "Back", fluidBuffer.Back);

		fluidSim.Dispatch(updateKernel, 1 + res / THREAD_GROUP_SIZE, 1 + res / THREAD_GROUP_SIZE, 1);

		//fluidSim.

		//print(fluidBuffer.Front.IsValid());

		fluid.SetHeight(fluidBuffer.Front);
	}

	public void Start(int size) {
		res = size + 1;
		print("START!");

		fluid = FluidGenerator.Create(GameManager.SIZE);

		fluidSim.SetInt("Res", res);

		fluidBuffer = new SwapBuffer(res * res, 4);



		Test(size);
	}

	private void Test(int size) {
		float[] data = new float[res * res];

		float total = 0;

		for (int i = 0; i < res * res; i++) {
			if ((new Vector2(res/2f, res/2f) - MeshHelper.UnflattenCoords(res, i)).magnitude < res/3f)
				data[i] = 60;
				total += data[i];
		}

		fluidBuffer.Front.SetData(data);
		fluidBuffer.Back.SetData(data);

		float height = total / (res * res);
		Vector3 pos;
		GameObject testWall;

		testWall = GameObject.CreatePrimitive(PrimitiveType.Cube); // North
		testWall.name = "North Wall";
		pos = MeshHelper.ArrToWorld(size + 1, 0, size + 1);
		pos.Scale(new Vector3(0, 0, 1));
		pos += new Vector3(0, height / 2, 0);
		testWall.transform.position = pos;
		testWall.transform.localScale = new Vector3(size + 2, height, 1);

		testWall = GameObject.CreatePrimitive(PrimitiveType.Cube); // East
		testWall.name = "East Wall";
		pos = MeshHelper.ArrToWorld(size + 1, size + 1, 0);
		pos.Scale(new Vector3(1, 0, 0));
		pos += new Vector3(0, height / 2, 0);
		testWall.transform.position = pos;
		testWall.transform.localScale = new Vector3(1, height, size + 2);

		testWall = GameObject.CreatePrimitive(PrimitiveType.Cube); // South
		testWall.name = "South Wall";
		pos = MeshHelper.ArrToWorld(size + 1, 0, 0);
		pos.Scale(new Vector3(0, 0, 1));
		pos += new Vector3(0, height / 2, 0);
		testWall.transform.position = pos;
		testWall.transform.localScale = new Vector3(size + 2, height, 1);

		testWall = GameObject.CreatePrimitive(PrimitiveType.Cube); // West
		testWall.name = "West Wall";
		pos = MeshHelper.ArrToWorld(size + 1, 0, 0);
		pos.Scale(new Vector3(1, 0, 0));
		pos += new Vector3(0, height / 2, 0);
		testWall.transform.position = pos;
		testWall.transform.localScale = new Vector3(1, height, size + 2);
	}

	public void Save() {

	}

	public void Load() {

	}


	class SwapBuffer
	{
		private bool isSwapped = false;

		private ComputeBuffer buffer1;
		private ComputeBuffer buffer2;

		public ComputeBuffer Front { get { return isSwapped ? buffer2 : buffer1; } }
		public ComputeBuffer Back { get { return isSwapped ? buffer1 : buffer2; } }

		public SwapBuffer(int count, int stride) {
			buffer1 = new ComputeBuffer(count, stride);
			buffer1.name = "SwapBuffer1";

			buffer2 = new ComputeBuffer(count, stride);
			buffer2.name = "SwapBuffer2";
		}

		public void Swap() {
			isSwapped = !isSwapped;
		}

		public void Destroy() {
			buffer1.Release();
			buffer2.Release();
		}
	}
}