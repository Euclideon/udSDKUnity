package com.euclideon;

import android.content.Context;

/**
    Euclideon Vault SDK library initialization
*/
public class VaultSDK {
    // This function should be called first and sets up the native code
    // so it can call into the Java classes
    public static native int nativeSetupJNI(Context context);

    public static void setupJNI(Context context) {
        nativeSetupJNI(context);
    }
}
