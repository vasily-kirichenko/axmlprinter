module axmlprinter.tests.XmlBeautifizerTests

open NUnit.Framework
open axmlprinter

[<Test>]
let beautifize () =
    let input = """<?xml version="1.0" encoding="utf-8"?>
<manifest
xmlns:android="http://schemas.android.com/apk/res/android"
package="com.rovio.angrybirdsrio">
<application

android:label="@7F030001"
android:icon="@7F020000"
android:debuggable="false">
<activity

android:theme="@android:01030007"
android:name="com.rovio.ka3d.App"
android:launchMode="2"
android:screenOrientation="0"
android:configChanges="0x000004A0">
<intent-filter

>
<action

android:name="android.intent.action.MAIN">
</action></intent-filter></activity></application></manifest>"""

    let expected = """<?xml version="1.0" encoding="utf-8"?>
<manifest xmlns:android="http://schemas.android.com/apk/res/android" package="com.rovio.angrybirdsrio">
  <application android:label="@7F030001" android:icon="@7F020000" android:debuggable="false">
    <activity android:theme="@android:01030007" android:name="com.rovio.ka3d.App" android:launchMode="2" android:screenOrientation="0" android:configChanges="0x000004A0">
      <intent-filter>
        <action android:name="android.intent.action.MAIN" />
      </intent-filter>
    </activity>
  </application>
</manifest>"""

    let actual = XmlBeautifizer.beautifize input
    printfn "actual:\n%s" actual

    Assert.That(actual, Is.EqualTo(expected))
