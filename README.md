# Narupa iMD

Interactive Molecular Dynamics (iMD) in VR, an application built with the Narupa
framework.

This repository is maintained by the Intangible Realities Laboratory, University Of Bristol,
and distributed under [GPLv3](LICENSE).
See [the list of contributors](CONTRIBUTORS.md) for the individual authors of the project.

# Run the latest development build

To run Narupa iMD, you need [SteamVR](https://www.steamvr.com/) installed.

* Download the [latest Windows build](https://github.com/IRL2/narupa-imd/releases/download/nightly/StandaloneWindows64.zip) from the [nightly release](https://github.com/IRL2/narupa-imd/releases/tag/nightly).
* Extract the downloaded zip file.
* In the extracted directory, launch `StandaloneWindows64.exe`. Windows will likely prompt you with a warning about the executable not being signed. If it happens, click on the "More info" button, then "Run anyway". You will also likely be prompted by the Windows firewall, allow Narupa to access the network.

# Installation with conda

If you've not already set up anaconda:

* Install Anaconda (avoid Anaconda 2.7 as it is outdated)
* Start the "Anaconda Powershell Prompt" where to type the next commands
* Create a conda environment (here we call the environment "narupa"): `conda create -n narupa "python>3.6"`
* Activate the conda environment: `conda activate narupa`

Then:

* Install the Narupa IMD package:

```
conda install -c irl narupa-imd
```

Run it! In the powershell prompt (Start Menu integration coming soon):

```
NarupaImd
```

# Installation for Development

*  Clone this repository to a folder on your computer.
*  Download Unity Hub by visiting the [Unity Download Page](https://unity3d.com/get-unity/download) and clicking the green **Download Unity Hub** button.
*  Install Unity Hub onto your computer.
*  Go to the [Unity Download Archive](https://unity3d.com/get-unity/download/archive) and click the green **Unity Hub** download button next to the label **2019.3.0** (the version required for this project)
*  Once installed, navigate to the **Projects** tab and click **Add** in the top right of Unity Hub.
*  Select the folder which you downloaded the repository to.

Once open in Unity, the main Unity scene can be found in `NarupaIMD/Assets/NarupaXR Scene`.

## Citation, Credits and External Libraries

If you find this project useful, please cite the following paper: 

M. O’Connor, S.J. Bennie, H.M. Deeks, A. Jamieson-Binnie, A.J. Jones, R.J. Shannon, R. Walters, T. Mitchell, A.J. Mulholland, D.R. Glowacki, [“Interactive molecular dynamics from quantum chemistry to drug binding: an open-source multi-person virtual reality framework”](https://aip.scitation.org/doi/10.1063/1.5092590), J. Chem Phys 150, 224703 (2019)

This project has been made possible by the following projects. We gratefully thank them for their efforts, and suggest that you use and cite them:

* [unity](https://unity.com/) - Development platform.
* [TextMeshPro](https://docs.unity3d.com/Packages/com.unity.textmeshpro@2.1/manual/index.html) - Text rendering for Unity.
* [gRPC](https://grpc.io/) (Apache v2) - Communication protocol.
* [SteamVR SDK](https://github.com/ValveSoftware/steamvr_unity_plugin) (BSD) - SDK for developing VR applications in SteamVR.
* The CIF file importer uses the *Chemical Component Dictionary* provided at http://www.wwpdb.org/data/ccd.
* [icons8](https://icons8.com) - Provider of icons used in UI.
* [NSubstitute](https://nsubstitute.github.io/) (BSD) - Mock testing framework.
