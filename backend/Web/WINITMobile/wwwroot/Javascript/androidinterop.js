window.androidInterop = {
    getFirebaseToken: async function () {
        // This function should invoke a C# method to retrieve the Firebase token
        const token = await DotNet.invokeMethodAsync('WINITMobile', 'WINITMobile.MainActivity.GetFirebaseToken');
        return token;
    },
    onAppResume: function () {
        // Code to execute when the app resumes
    }

};
