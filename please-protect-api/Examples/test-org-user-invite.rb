#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = ENV['API_ORG']

param =  {
  UserName: "#{ENV['USER_NAME']}",
  TmpUserEmail: "pjame.fb1@gmail.com",
  Roles: [ "OWNER" ],
}

### Inviteuser
apiUrl = "api/OrganizationUser/org/#{orgId}/action/InviteUser"
result = make_request(:post, apiUrl, param)
puts(result)

