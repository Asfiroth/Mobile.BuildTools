The first time the Mobile.BuildTools encounters an image it will automatically generate a default image configuration file along side of the image. This file is what allows you to customize and further refine the image that you want. A configuration file may reside a conditional search directory. The Mobile.BuildTools will opt to use a configuration that is not in the same file directory as the image resource any time that a duplication is found.

!!! warning Warning
    Keep in mind that we only ever support a scenario where you have a single configuration in a directory other than the original image. In the event that two or more configurations are found for the same image in directories other than the directory where the image is located the Mobile.BuildTools will throw an exception causing a Build Error.

!!! note Note
    If you have an image name that contains white space such as `foo bar.png`, the Mobile.BuildTools will sanitize the file name replace any contiguous white space characters with a single `-`. This normalization is required by Android and will mean that you will need to reference the image as `foo-bar.png`.

## Configuring Images

The Schema for configuring images is rather simple by design. We allow you to specify a watermark file name, a name, a scale and an optional ignore. To start let's consider that we have an image named `Mobile-BuildTools.png`. We know that we cannot get away with this file name on all platforms so we want to rename the generated image.

```json
{
  "name": "mobile_buildtools"
}
```

The above sample would allow us to have a resource named `mobile_buildtools` when we refer to this from our Xamarin code.

## Watermarking Images

One of the great things that the Mobile.BuildTools supports is watermarking images at build. This is a very powerful feature as shown below. To configure images for Watermarking there are a few simple steps that you will need to take.

Let's first make a couple of assumptions.

- You have your base resources in the path `Images`
- This includes an image asset called `icon.png`
- You have a subfolder called `Images\Debug`
- Inside of this directory you have a watermark file called `beta-version.png`

In order to set this up our `buildtools.json` might look like this:

```json
{
  "images": {
    "directories" : [ "Images" ],
    "conditionalDirectories": {
      "Debug": [ "Images\\Debug" ]
    }
  }
}
```

| icon.png | beta-version.png | output: icon.png |
|:--------:|:----------------:|:----------------:|
| ![Mobile.BuildTools](/assets/samples/icon.png "Mobile.BuildTools") | ![Dev Badge](/assets/samples/beta-version.png "beta version") | ![Mobile.BuildTools - Dev](/assets/samples/icon-beta.png "Mobile.BuildTools - Dev") |

When the Mobile.BuildTools runs and the Image collector locates an image, it will automatically generate a default JSON configuration file. The configuration file must have the same name as the image without the file extension. As a result after the first run our output image will look exactly the same as our input image only resized, and we would also have an asset added for the `beta-version.png` although this was not what we had wanted. The file system will also now contain the new files `Images\icon.json` & `Images\Debug\beta-version.json`.

You will not need to do anything to the `Images\icon.json` file, however you will want to copy this to `Images\Debug` directory. Next you'll want to open a text editor such as Visual Studio Code which will automatically pick up the schema referenced in the generated file and give you intellisense as you work on the file. We'll update the following two files as follows:

**Images\Debug\beta-version.json**

```json
{
  "$schema": "http://mobilebuildtools.com/schemas/v2/resourceDefinition.schema.json",
  "ignore": true
}
```

**Images\Debug\icon.json**

```json
{
  "$schema": "http://mobilebuildtools.com/schemas/v2/resourceDefinition.schema.json",
  "watermarkFile": "beta-version"
}
```

!!! warning
    When using Conditional Directories to modify images with watermarks be sure that there is never more than one conditional configuration included. Doing so will result in a build error as the Mobile.BuildTools has no way of know which conditional configuration to use.

With our updated configurations we can now rebuild and the Mobile.BuildTools will generally ignore the `beta-version.png` as an asset of it's own while it will 

