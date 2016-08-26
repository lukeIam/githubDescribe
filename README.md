# githubDescribe
git describe --long without cloning the repository (uses GitHub API)

## PowerShell version
[github-describe-ps folder](https://github.com/lukeIam/githubDescribe/tree/master/github-describe-ps)

- Set `$currentCommitSha` and `$githubRepoApiUri` OR `$Env:CurrentCommitSha` and `$Env:githubRepoApiUri`
- Run the script
- Get version tag as return value or in `$Env:VersionTag`

## .NET 4 Console Application version
[github-describe-net folder](https://github.com/lukeIam/githubDescribe/tree/master/github-describe-net)

Usage:
```
githubDescribe.exe ProjectOwner Project CommitSHA
```
