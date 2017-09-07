using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class SmoothedNumber
{
    public float average { get; private set; }
    double total = 0;
    float[] readings;
    int nextValueSlot = 0;
    bool firstTime = true;

    public SmoothedNumber(int bufferSize)
    {
        readings = new float[bufferSize];
        total = 0;
        average = 0;
    }

    public void Clear()
    {
        for(int Idx = 0; Idx < readings.Length; ++Idx)
            readings[Idx] = 0.0f;
        total = 0;
        average = 0;
    }

    public void AddReading(float newReading)
    {
        if (firstTime)
        {
            total += newReading;
            average = (float)(total / (nextValueSlot + 1));
        }
        else
        {
            total -= readings[nextValueSlot];
            total += newReading;
            average = (float)(total / readings.Length);
        }

        readings[nextValueSlot] = newReading;
        nextValueSlot++;
        if (nextValueSlot >= readings.Length)
        {
            firstTime = false;
            nextValueSlot = 0;
        }
    }
}

class SmoothedVector2
{
    SmoothedNumber x;
    SmoothedNumber y;

    public Vector3 average { get { return new Vector2(x.average, y.average); } }

    public SmoothedVector2(int bufferSize)
    {
        x = new SmoothedNumber(bufferSize);
        y = new SmoothedNumber(bufferSize);
    }

    public void AddReading(Vector2 newReading)
    {
        x.AddReading(newReading.x);
        y.AddReading(newReading.y);
    }
}

class SmoothedVector3
{
    SmoothedNumber x;
    SmoothedNumber y;
    SmoothedNumber z;

    public Vector3 average { get { return new Vector3(x.average, y.average, z.average); } }

    public SmoothedVector3(int bufferSize)
    {
        x = new SmoothedNumber(bufferSize);
        y = new SmoothedNumber(bufferSize);
        z = new SmoothedNumber(bufferSize);
    }

    public void AddReading(Vector3 newReading)
    {
        x.AddReading(newReading.x);
        y.AddReading(newReading.y);
        z.AddReading(newReading.z);
    }

    public void Clear()
    {
        x.Clear();
        y.Clear();
        z.Clear();
    }
}