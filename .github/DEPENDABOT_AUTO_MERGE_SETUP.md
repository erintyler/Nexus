# Dependabot Auto-Merge Setup

This repository is configured to automatically merge Dependabot PRs for **patch** and **minor** updates after all CI checks pass.

## 🤖 How It Works

1. **Dependabot creates a PR** for a dependency update
2. **CI pipeline runs** (formatting, build, architecture tests, unit tests, integration tests)
3. **Fastify's merge-dependabot action** checks the update type:
   - **Patch/Minor**: Automatically approves and enables GitHub's native auto-merge
   - **Major**: Skipped (requires manual review)
4. **GitHub auto-merges** the PR once all required checks pass

**Technology Used:** [Fastify's github-action-merge-dependabot v3](https://github.com/fastify/github-action-merge-dependabot)

## ⚙️ Required Setup

### 1. Enable Auto-Merge in Repository Settings

1. Go to **Settings** → **General**
2. Scroll to **Pull Requests** section
3. Enable **"Allow auto-merge"**

### 2. Configure Branch Protection Rules

To ensure PRs only merge after tests pass, configure branch protection:

1. Go to **Settings** → **Branches**
2. Click **Add rule** for `main` branch
3. Configure the following:

   **Required Settings:**
   - ✅ **Require a pull request before merging**
     - ✅ Require approvals: **1**
   - ✅ **Require status checks to pass before merging**
     - ✅ Require branches to be up to date before merging
     - Add required status checks:
       - `Formatting Checks`
       - `Build Solution`
       - `🏗️ Architecture Tests`
       - `🧪 Unit Tests`
       - `🔗 Integration Tests`
   - ✅ **Do not allow bypassing the above settings**

   **Optional (Recommended):**
   - ✅ Require conversation resolution before merging
   - ✅ Require linear history

4. Click **Create** or **Save changes**

### 3. Grant GitHub Actions Write Permissions

1. Go to **Settings** → **Actions** → **General**
2. Scroll to **Workflow permissions**
3. Select **"Read and write permissions"**
4. Enable **"Allow GitHub Actions to create and approve pull requests"**
5. Click **Save**

## 📋 Update Types

| Update Type | Behavior |
|------------|----------|
| **Patch** (e.g., 1.2.3 → 1.2.4) | ✅ Auto-approved and auto-merged |
| **Minor** (e.g., 1.2.0 → 1.3.0) | ✅ Auto-approved and auto-merged |
| **Major** (e.g., 1.0.0 → 2.0.0) | ⏭️ Skipped - manual review required |

## ⚙️ Workflow Configuration

The workflow uses the following settings:

```yaml
target: minor              # Auto-merge patch and minor updates
merge-method: squash      # Squash commits on merge
approve-only: false       # Actually merge, not just approve
use-github-auto-merge: true  # Wait for all checks to pass
```

**Key Feature:** `use-github-auto-merge: true` ensures the PR only merges after **all required status checks pass**.

## 📦 Dependabot Configuration

- **Schedule**: Weekly on Mondays at 09:00 UTC
- **Ecosystems**: NuGet packages and GitHub Actions
- **Grouping**: Minor and patch updates are grouped to reduce PR noise

## 🔒 Security

- Only Dependabot PRs are auto-merged (verified by `github.actor == 'dependabot[bot]'`)
- Uses `pull_request_target` for secure workflow execution
- Major version updates are skipped and require manual review
- All CI checks must pass before merge (enforced by GitHub's auto-merge)
- Branch protection prevents direct pushes to main

## 🛠️ Testing the Setup

To test the auto-merge workflow:

1. Create a test Dependabot PR or wait for the next scheduled run
2. Verify the workflow runs and approves the PR
3. Check that auto-merge is enabled
4. Wait for all CI checks to complete
5. PR should auto-merge once checks pass

## 📝 Manual Override

To prevent auto-merge on a specific Dependabot PR:

1. **Before workflow runs**: Add a blocking review or label
2. **After auto-merge enabled**: Disable auto-merge manually
   ```bash
   gh pr merge --disable-auto <PR_NUMBER>
   ```
3. **For major updates**: They are automatically skipped by the workflow

To manually merge a major update after review:
```bash
gh pr review --approve <PR_NUMBER>
gh pr merge --squash <PR_NUMBER>
```

## 🚨 Troubleshooting

**Auto-merge not working?**
- Verify repository has auto-merge enabled in Settings
- Check branch protection rules are configured with required status checks
- Ensure GitHub Actions has write permissions for PRs and contents
- Verify all required status checks are passing
- Check the PR is for a patch or minor update (not major)

**Workflow not running?**
- Check the PR is from `dependabot[bot]`
- Verify workflow file is in `.github/workflows/dependabot-auto-merge.yml`
- Ensure workflow uses `pull_request_target` trigger (not `pull_request`)
- Check Actions tab for workflow execution logs

**Action fails with permissions error?**
- Go to Settings → Actions → General → Workflow permissions
- Enable "Read and write permissions"
- Enable "Allow GitHub Actions to create and approve pull requests"

**Want to see what's happening?**
- Check the Actions tab for the "Dependabot Auto-Merge" workflow
- Review the Fastify action logs for detailed information

## 📚 References

- [Fastify's github-action-merge-dependabot](https://github.com/fastify/github-action-merge-dependabot)
- [GitHub Auto-merge Documentation](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/incorporating-changes-from-a-pull-request/automatically-merging-a-pull-request)
- [Dependabot Configuration Reference](https://docs.github.com/en/code-security/dependabot/dependabot-version-updates/configuration-options-for-the-dependabot.yml-file)
- [Branch Protection Rules](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-protected-branches/about-protected-branches)


