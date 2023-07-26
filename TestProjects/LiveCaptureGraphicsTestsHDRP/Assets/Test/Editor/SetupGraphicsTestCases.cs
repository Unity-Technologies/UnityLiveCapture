using UnityEngine.TestTools;

public class SetupGraphicsTestCases : IPrebuildSetup
{
    public void Setup()
    {
        UnityEditor.TestTools.Graphics.SetupGraphicsTestCases.Setup(GraphicsTests.Path);
    }
}
