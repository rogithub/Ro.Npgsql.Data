# Ro.Npgsql.Data

[![NuGet Badge](https://img.shields.io/nuget/v/Ro.Npgsql.Data.svg?style=flat-square)](https://www.nuget.org/packages/Ro.Npgsql.Data)

Postgres data layer for C#.

---

## Creating a New Release

To create a new release of your package, follow these steps:

1. **Update the Version Number**
   
   In your project, update the version number in the `.csproj` file. Locate the `<Version>` tag and increment the version:

``` xml
<PropertyGroup>
    <Version>1.0.2</Version> <!-- Increment the version number -->
</PropertyGroup>
```
2. **Tag the Release in Git**
   
   After updating the version number, tag your release in Git. This will trigger the GitHub Actions workflow to automatically build and publish the package.

   First, commit the changes:

``` bash
git add . # Stage all changes
git commit -m "Release version 1.0.2" # Commit with an appropriate message
```

   Then, create a Git tag for the new version:
``` bash
git tag v1.0.2 # Replace with your version number

```

   Push the commit and tag to GitHub:
``` bash
git push origin main # Push the commit
git push origin v1.0.2 # Push the tag
```

3. **GitHub Actions Will Automate the Process**
   
   Once you push the tag, the GitHub Actions workflow defined in ```.github/workflows/nuget-publish.yml``` will automatically trigger the build and publish the new version of the package to GitHub Packages.

   You can monitor the progress of the workflow in the Actions tab of your GitHub repository.
