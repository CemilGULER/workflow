
public abstract record PostWorkflowDefinitionRequest(string name, MultilanguageText[] title, PostWorkflowEntity[] entities, string[] tags);

public record PostFSMWorkflowDefinitionRequest(string name, MultilanguageText[] title, PostWorkflowEntity[] entities, string[] tags) : PostWorkflowDefinitionRequest(name, title, entities, tags);
public record PostZeebeWorkflowDefinitionRequest(string name, MultilanguageText[] title, PostWorkflowEntity[] entities, string[] tags, string process, string gateway) : PostWorkflowDefinitionRequest(name, title, entities, tags);

public abstract record PostWorkflowDefinitionResponse(string name);

public record PostWorkflowEntity(string name, bool isExclusive, bool isStateManager, BaseStatusType[] availableInStatus);