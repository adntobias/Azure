using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Linq;

namespace TEATEM
{
  public class GraphHelper
  {
    private static GraphServiceClient graphClient;

    public static void Initialize(IAuthenticationProvider authProvider)
    {
      graphClient = new GraphServiceClient(authProvider);
    }

    public static async Task<Group> CreateNewGroup(string name)
    {
      Group newGroup = null;

      try
      {
        var group = new Group
        {
          Description = "Sample Group for MS600",
          DisplayName = name,
          GroupTypes = new List<String>()
            {
                "Unified"
            },
          MailEnabled = true,
          MailNickname = name.Replace(" ", ""),
          SecurityEnabled = false
        };

        newGroup = await graphClient.Groups.Request().AddAsync(group);
      }
      catch (ServiceException ex)
      {
        Console.WriteLine($"Error creating group: {ex.Message}");
      }

      return newGroup;
    }

    public static async Task AddMembers(string _id)
    {
      var additionalDataTemplate = new Dictionary<string, object>(){
        /*  {"owners@odata.bind", new List<string>()}, */
        {"members@odata.bind", new List<string>()}
      };

      string OdataTemplate = @"https://graph.microsoft.com/v1.0/directoryObjects/{0}";

      List<DirectoryObject> membersList = new List<DirectoryObject>();
      membersList.Add(new DirectoryObject { Id = "13fcd60c-5bb9-4cd4-a18c-79dde8727a4c" });
      membersList.Add(new DirectoryObject { Id = "ebb64066-744e-47d8-a822-b94f7943bf98" });
      membersList.Add(new DirectoryObject { Id = "dd235208-bcc1-47d5-b2f1-219c240f6180" });

      foreach (var member in membersList)
      {
        (additionalDataTemplate["members@odata.bind"] as List<string>).Add(String.Format(OdataTemplate, member.Id));
      }

      var group = new Group
      {
        AdditionalData = additionalDataTemplate
      };

      var owner = new DirectoryObject { Id = "6ae941d0-1625-4182-8a61-30d58f83a092" };

      try
      {
        await graphClient.Groups[_id].Owners.References.Request().AddAsync(owner);
        await graphClient.Groups[_id].Request().UpdateAsync(group);
      }
      catch (ServiceException ex)
      {
        Console.WriteLine($"Error adding members: {ex.Message}");
      }
    }

    public static async Task<Team> CreateNewTeam(string name)
    {
      string teamId = null;
      var team = new Team
      {
        DisplayName = name,
        Description = "My Sample Team’s Description",
        Visibility = TeamVisibilityType.Public,
        Channels = new TeamChannelsCollectionPage()
        {
          new Channel
          {
            DisplayName = "Announcements 📢",
            IsFavoriteByDefault = true,
            Description = "This is a sample announcements channel that is favorited by default. Use this channel to make important team, product, and service announcements."
          }
        },
        MemberSettings = new TeamMemberSettings
        {
          AllowCreateUpdateChannels = true,
          AllowDeleteChannels = true,
          AllowAddRemoveApps = true,
          AllowCreateUpdateRemoveTabs = true,
          AllowCreateUpdateRemoveConnectors = true
        },
        GuestSettings = new TeamGuestSettings
        {
          AllowCreateUpdateChannels = false,
          AllowDeleteChannels = false
        },
        FunSettings = new TeamFunSettings
        {
          AllowGiphy = true,
          GiphyContentRating = GiphyRatingType.Moderate,
          AllowStickersAndMemes = true,
          AllowCustomMemes = true
        },
        MessagingSettings = new TeamMessagingSettings
        {
          AllowUserEditMessages = true,
          AllowUserDeleteMessages = true,
          AllowOwnerDeleteMessages = true,
          AllowTeamMentions = true,
          AllowChannelMentions = true
        },
        AdditionalData = new Dictionary<string, object>()
        {
          {"template@odata.bind", "https://graph.microsoft.com/v1.0/teamsTemplates('standard')"}
        },
        Members = new TeamMembersCollectionPage()
        {
          new AadUserConversationMember
          {
            Roles = new List<String>()
            {
              "owner"
            },
            AdditionalData = new Dictionary<string, object>()
            {
              {"user@odata.bind", "https://graph.microsoft.com/v1.0/users('6ae941d0-1625-4182-8a61-30d58f83a092')"}
            }
          }
        },
        InstalledApps = new TeamInstalledAppsCollectionPage()
        {
          new TeamsAppInstallation
          {
            AdditionalData = new Dictionary<string, object>()
            {
              {"teamsApp@odata.bind", "https://graph.microsoft.com/v1.0/appCatalogs/teamsApps('com.microsoft.teamspace.tab.vsts')"}
            }
          },
          new TeamsAppInstallation
          {
            AdditionalData = new Dictionary<string, object>()
            {
              {"teamsApp@odata.bind", "https://graph.microsoft.com/v1.0/appCatalogs/teamsApps('1542629c-01b3-4a6d-8f76-1938b779e48d')"}
            }
          }
        }
      };
      
      try
      {
        //Unfortunately, this always returns 'null', so we have to do it more complicated >:(
        //newTeam = await graphClient.Teams.Request().AddAsync(team);

        //StackOverflow: 64667655
        BaseRequest request = (BaseRequest)graphClient.Teams.Request();
        request.ContentType = "application/json";
        request.Method = "POST";

        string location;
        using (HttpResponseMessage response = await request.SendRequestAsync(team, CancellationToken.None))
        {
          location = response.Headers.Location.ToString();

          // looks like: /teams('7070b1fd-1f14-4a06-8617-254724d63cde')/operations('c7c34e52-7ebf-4038-b306-f5af2d9891ac')
          // but is documented as: /teams/7070b1fd-1f14-4a06-8617-254724d63cde/operations/c7c34e52-7ebf-4038-b306-f5af2d9891ac
          // -> this split supports both of them
          string[] locationParts = location.Split(new[] { '\'', '/', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
          teamId = locationParts[1];
          string operationId = locationParts[3];

          // before querying the first time we must wait some secs, else we get a 404
          int delayInMilliseconds = 5_000;
          while (true)
          {
            await Task.Delay(delayInMilliseconds);

            // lets see how far the teams creation process is
            TeamsAsyncOperation operation = await graphClient.Teams[teamId].Operations[operationId].Request().GetAsync();
            if (operation.Status == TeamsAsyncOperationStatus.Succeeded)
              break;

            if (operation.Status == TeamsAsyncOperationStatus.Failed)
              throw new Exception($"Failed to create team '{team.DisplayName}': {operation.Error.Message} ({operation.Error.Code})");

            // according to the docs, we should wait > 30 secs between calls
            // https://docs.microsoft.com/en-us/graph/api/resources/teamsasyncoperation?view=graph-rest-1.0
            delayInMilliseconds = 30_000;
          }

          //we can now work with the Id
          team.Id = teamId;
        }
      }
      catch (ServiceException ex)
      {
        Console.WriteLine($"Error creating team: {ex.Message}");
      }
      
      return team;
    }

    public static async Task<Team> ChangeGroupToTeam(string _id)
    {
      Team newTeam = null;

      try
      {
        var team = new Team
        {
          MemberSettings = new TeamMemberSettings
          {
            AllowCreatePrivateChannels = true,
            AllowCreateUpdateChannels = true,
            ODataType = null
          },
          MessagingSettings = new TeamMessagingSettings
          {
            AllowUserEditMessages = true,
            AllowUserDeleteMessages = true,
            ODataType = null
          },
          FunSettings = new TeamFunSettings
          {
            AllowGiphy = true,
            GiphyContentRating = GiphyRatingType.Strict,
            ODataType = null
          },
          ODataType = null
        };

        newTeam = await graphClient.Groups[_id].Team.Request().PutAsync(team);
      }
      catch (ServiceException ex)
      {
        if(ex.StatusCode != HttpStatusCode.NotFound){
          Console.WriteLine($"Error change group to team: {ex.Message}");
        }
      }

      return newTeam;
    }

    public static async Task<List<ConversationMember>> ListTeamMembers(string _id)
    {
      List<ConversationMember> members = new List<ConversationMember>();

      try
      {
        var _members = await graphClient.Teams[_id].Members.Request().GetAsync();

        while (_members.Count > 0)
        {
          members.AddRange(_members);

          if (_members.NextPageRequest == null)
          {
            break;
          }

          _members = await _members.NextPageRequest.GetAsync();
        }
      }
      catch (ServiceException ex)
      {
        Console.WriteLine($"Error listing members: {ex.Message}");
      }

      return members;
    }

    public static async Task<List<Team>> ListTeams()
    {
      var teams = new List<Team>();

      try
      {
        var _teams = await graphClient.Teams.Request().GetAsync();

        while (_teams.Count > 0)
        {
          teams.AddRange(_teams);

          if (_teams.NextPageRequest == null)
          {
            break;
          }

          _teams = await _teams.NextPageRequest.GetAsync();
        }
      }
      catch (ServiceException ex)
      {
        Console.WriteLine($"Error list teams: {ex.Message}");
      }

      return teams;
    }

    public static async Task DeleteTeam(string _id)
    {
      try
      {
        //await graphClient.Teams[_id].Request().DeleteAsync();
        await graphClient.Groups[_id].Request().DeleteAsync(); //more stable
        //await graphClient.Teams[_id].Archive(null).Request().PostAsync(); //only archive (Read-only)
      }
      catch (ServiceException ex)
      {
        Console.WriteLine($"Error deleting team: {ex.Message}");
      }
    }

    public static async Task<Dictionary<Channel, List<TeamsTab>>> ListTeamChannelAndTabs(string _id)
    {
      Dictionary<Channel, List<TeamsTab>> dictResult = new Dictionary<Channel, List<TeamsTab>>();

      try
      {
        var channels = await graphClient.Teams[_id].Channels
        .Request()
        .Select(c => new
        {
          c.Id,
          c.DisplayName,
          c.Tabs,
          c.WebUrl
        })
        .GetAsync();

        foreach (Channel ch in channels)
        {
          List<TeamsTab> listTabs = new List<TeamsTab>();

          var tabs = await graphClient.Teams[_id].Channels[ch.Id].Tabs.Request().GetAsync();

          foreach (TeamsTab t in tabs)
          {
            listTabs.Add(t);
          }

          dictResult.Add(ch, listTabs);
        }
      }
      catch (ServiceException ex)
      {
        Console.WriteLine($"Error listing channels/tabs: {ex.Message}");
      }

      return dictResult;
    }

    public static async Task AddTeamsChannelAndTab(string _id)
    {
      try
      {
        var channel = new Channel
        {
          DisplayName = "API Discussion",
          Description = "This channel is where we debate all API things",
          MembershipType = ChannelMembershipType.Standard
        };

        var newChannel = await graphClient.Teams[_id].Channels.Request().AddAsync(channel);

        var apps = await graphClient.Teams[_id].InstalledApps.Request().Expand("TeamsApp").GetAsync();
        var WebApp = apps.Where(x => x.TeamsApp.DisplayName == "Website").Select(y => y.TeamsApp).FirstOrDefault();

        var tab = new TeamsTab
        {
          DisplayName = "My New Tab",
          TeamsApp = WebApp,
          Configuration = new TeamsTabConfiguration
          {
            ContentUrl = "https://github.com/microsoftgraph/microsoft-graph-docs/blob/main/api-reference/v1.0/overview.md",
            EntityId = "TotallyUniqueId",
            RemoveUrl = null,
            WebsiteUrl = null,
            ODataType = null
          },
          AdditionalData = new Dictionary<string, object>()
          {
              { "teamsApp@odata.bind", "https://graph.microsoft.com/beta/appCatalogs/teamsApps/com.microsoft.teamspace.tab.web" }
          }
        };

        await graphClient.Teams[_id].Channels[newChannel.Id].Tabs.Request().AddAsync(tab);
      }
      catch (ServiceException ex)
      {
        Console.WriteLine($"Error adding Channel/Tab: {ex.Message}");
      }
    }
  }
}