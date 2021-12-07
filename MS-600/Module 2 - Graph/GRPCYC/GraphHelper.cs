using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GRPCYC
{
  public class GraphHelper
  {
    private static GraphServiceClient graphClient;
    
    public static void Initialize(IAuthenticationProvider authProvider)
    {
      graphClient = new GraphServiceClient(authProvider);
    }

    public static async Task<List<Group>> GetGroupsAsync()
    {
      var groups = new List<Group>();

      try
      {
        var _groups = await graphClient.Groups
            .Request()
            .Select(u => new
            {
              u.Id,
              u.DisplayName,
              u.CreatedDateTime,
              u.Members,
            })
            .GetAsync();

        while (_groups.Count > 0)
        {
          groups.AddRange(_groups);

          if (_groups.NextPageRequest == null)
          {
            break;
          }

          _groups = await _groups.NextPageRequest.GetAsync();
        }
      }
      catch (ServiceException ex)
      {
        Console.WriteLine($"Error getting groups: {ex.Message}");
      }

      return groups;
    }

    public static async Task<List<User>> GetMembersForGroupWithId(string _id)
    {
      var members = new List<User>();

      try
      {
        var _members = await graphClient.Groups[_id].Members
            .Request()
            .GetAsync();

        while (_members.Count > 0)
        {
          foreach (var m in _members)
          {
            members.Add(await graphClient.Users[m.Id].Request().Select(u => new { u.Id, u.DisplayName }).GetAsync());
          }

          if (_members.NextPageRequest == null)
          {
            break;
          }

          _members = await _members.NextPageRequest.GetAsync();
        }
      }
      catch (ServiceException ex)
      {
        Console.WriteLine($"Error getting members: {ex.Message}");
      }

      return members;
    }

    public static async Task<Group> CreateNewGroup(string name)
    {
      Group newGroup = null;

      try
      {
        var group = new Group{
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

      var group = new Group{
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

    public static async Task DeleteGroup(string _id)
    {
      try
      {
        await graphClient.Groups[_id].Request().DeleteAsync();
      }
      catch (ServiceException ex)
      {
        Console.WriteLine($"Error deleting group: {ex.Message}");
      }
    }
  }
}