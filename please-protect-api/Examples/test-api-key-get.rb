#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = ENV['API_ORG']
id = 'c916e634-19a9-48e9-97c1-3d1a1455eb6e'
param = {
  FullTextSearch: ""
}

### GetApiKeyById
apiUrl = "api/ApiKey/org/#{orgId}/action/GetApiKeys"
result = make_request(:post, apiUrl, param)
puts(result)
