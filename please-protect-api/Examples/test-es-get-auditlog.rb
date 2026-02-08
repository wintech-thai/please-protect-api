#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = ENV['API_ORG']
keyword = ''

apiUrl = "api/Es/org/#{orgId}/action/Proxy/pp-*/_search"
param = {
  query: {
    multi_match: {
      query: keyword,
      fields: ["message", "title", "description"]
    }
  },
  size: 20
}

result = make_request(:post, apiUrl, param)

json = result.to_json
puts(json)
